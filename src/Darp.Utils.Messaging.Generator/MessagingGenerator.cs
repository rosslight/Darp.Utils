namespace Darp.Utils.Messaging.Generator;

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable CA1031 // Do not catch general exception types -> better than throwing an exception

[Generator(LanguageNames.CSharp)]
public class MessagingGenerator : IIncrementalGenerator
{
    public const string MessageSinkAttributeName = "Darp.Utils.Messaging.MessageSinkAttribute";
    public const string MessageSourceAttributeName = "Darp.Utils.Messaging.MessageSourceAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Target MessageSink attribute
        IncrementalValuesProvider<SinkMethodInfo> messageSinkProvider =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                MessageSinkAttributeName,
                static (node, _) => node is MethodDeclarationSyntax,
                GetSinkMethodInfo
            );
        context.RegisterSourceOutput(messageSinkProvider.Collect(), ExecuteSinkMethods);

        // Target MessageSink attribute
        IncrementalValuesProvider<SourceTypeInfo> messageSourceProvider =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                MessageSourceAttributeName,
                static (node, _) =>
                    node is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax,
                GetSourceTypeInfo
            );
        context.RegisterSourceOutput(messageSourceProvider, ExecuteSourceTypes);
    }

    private static void ExecuteSinkMethods(
        SourceProductionContext spc,
        ImmutableArray<SinkMethodInfo> targetMethodInfos
    )
    {
        foreach (IGrouping<string, SinkMethodInfo> methodInfos in targetMethodInfos.GroupBy(x => x.HintName))
        {
            try
            {
                var fileName = $"{methodInfos.Key}.g.cs";

                var success = SinkEmitter.TryEmit(
                    methodInfos.ToArray(),
                    out var code,
                    out List<Diagnostic> diagnostics
                );
                foreach (Diagnostic diagnostic in diagnostics)
                    spc.ReportDiagnostic(diagnostic);
                if (!success)
                    continue;

                spc.AddSource(
                    fileName,
                    SourceText.From(code ?? string.Empty, Encoding.UTF8, SourceHashAlgorithm.Sha256)
                );
            }
            catch (Exception e)
            {
                spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GeneralError, null, e.Message));
            }
        }
    }

    private static void ExecuteSourceTypes(SourceProductionContext spc, SourceTypeInfo targetMethodInfo)
    {
        try
        {
            var fileName = $"{targetMethodInfo.HintName}.g.cs";

            var success = SourceEmitter.TryEmit(targetMethodInfo, out var code, out List<Diagnostic> diagnostics);
            foreach (Diagnostic diagnostic in diagnostics)
                spc.ReportDiagnostic(diagnostic);
            if (!success)
                return;

            spc.AddSource(fileName, SourceText.From(code ?? string.Empty, Encoding.UTF8, SourceHashAlgorithm.Sha256));
        }
        catch (Exception e)
        {
            spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GeneralError, null, e.Message));
        }
    }

    private static SinkMethodInfo GetSinkMethodInfo(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        var compilation = context.SemanticModel.Compilation as CSharpCompilation;
        AttributeData attribute = context.Attributes.First(x => x.ToString() == MessageSinkAttributeName);
        var compiledWithNet9OrGreater =
            compilation?.SyntaxTrees[0].Options.PreprocessorSymbolNames.Contains("NET9_0_OR_GREATER") is true;
        var type = (IMethodSymbol)context.TargetSymbol;
        return new SinkMethodInfo(type, attribute, compiledWithNet9OrGreater);
    }

    private static SourceTypeInfo GetSourceTypeInfo(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        AttributeData attribute = context.Attributes.First(x => x.ToString() == MessageSourceAttributeName);
        var type = (INamedTypeSymbol)context.TargetSymbol;
        return new SourceTypeInfo(type, attribute);
    }
}

internal readonly record struct SinkMethodInfo(
    IMethodSymbol Symbol,
    AttributeData SinkAttributeData,
    bool IsCompiledWithNet9OrGreater
)
{
    public string HintName { get; } = Symbol.ContainingType.GetFileName();
}

internal readonly record struct SourceTypeInfo(INamedTypeSymbol Symbol, AttributeData SinkAttributeData)
{
    public string HintName { get; } = Symbol.GetFileName();
}
