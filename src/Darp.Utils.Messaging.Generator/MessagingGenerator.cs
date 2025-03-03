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
    private const string MessageSinkAttributeName = "Darp.Utils.Messaging.MessageSinkAttribute";

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
            var fileName = $"{methodInfos.Key}.g.cs";

            spc.AddSource(
                fileName,
                SourceText.From("partial class TestClass;", Encoding.UTF8, SourceHashAlgorithm.Sha256)
            );
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
    public string HintName { get; } = Symbol.ContainingType.ToDisplayString();
}
