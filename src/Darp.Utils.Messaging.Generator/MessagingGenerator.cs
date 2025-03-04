namespace Darp.Utils.Messaging.Generator;

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class MessagingGenerator : IIncrementalGenerator
{
    public const string MessageSinkAttributeName = "Darp.Utils.Messaging.MessageSinkAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Only target specific attributes
        IncrementalValuesProvider<TargetMethodInfo> messageSinkProvider =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                MessageSinkAttributeName,
                static (node, _) => node is MethodDeclarationSyntax,
                GetMethodeInfo
            );
        context.RegisterSourceOutput(messageSinkProvider.Collect(), Execute);
    }

    private static void Execute(SourceProductionContext spc, ImmutableArray<TargetMethodInfo> targetMethodInfos)
    {
        foreach (IGrouping<string, TargetMethodInfo> methodInfos in targetMethodInfos.GroupBy(x => x.HintName))
        {
            try
            {
                var fileName = $"{methodInfos.Key}.g.cs";

                var success = Emitter.TryEmit(methodInfos.ToArray(), out var code, out List<Diagnostic> diagnostics);
                foreach (Diagnostic diagnostic in diagnostics)
                    spc.ReportDiagnostic(diagnostic);
                if (!success)
                    continue;

                spc.AddSource(
                    fileName,
                    SourceText.From(code ?? string.Empty, Encoding.UTF8, SourceHashAlgorithm.Sha256)
                );
            }
#pragma warning disable CA1031 // Do not catch general exception types -> better than throwing an exception
            catch (Exception e)
#pragma warning restore CA1031
            {
                spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GeneralError, null, e.Message));
            }
        }
    }

    private static TargetMethodInfo GetMethodeInfo(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var compilation = context.SemanticModel.Compilation as CSharpCompilation;
        LanguageVersion languageVersion = compilation?.LanguageVersion ?? LanguageVersion.CSharp1;

        var type = (IMethodSymbol)context.TargetSymbol;
        var node = (MethodDeclarationSyntax)context.TargetNode;
        return new TargetMethodInfo(type, node, languageVersion);
    }
}

internal readonly record struct TargetMethodInfo(
    IMethodSymbol Symbol,
    MethodDeclarationSyntax Syntax,
    LanguageVersion LanguageVersion
)
{
    public string HintName { get; } = GetFileName(Symbol.ContainingType);

    public AttributeData SinkAttributeData =>
        Symbol
            .GetAttributes()
            .First(x => x.AttributeClass?.ToDisplayString() == MessagingGenerator.MessageSinkAttributeName);

    private static string GetFileName(INamedTypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : typeSymbol.ContainingNamespace.ToDisplayString() + ".";

        var name = typeSymbol.Name;

        if (typeSymbol.TypeArguments.Length <= 0)
            return ns + name;

        var typeArgNames = string.Join("_", typeSymbol.TypeArguments.Select(arg => arg.Name));
        return ns + name + "_" + typeArgNames;
    }
}
