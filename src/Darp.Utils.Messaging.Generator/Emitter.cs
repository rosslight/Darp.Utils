namespace Darp.Utils.Messaging.Generator;

using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using MethodInfo = (
    bool IsAny,
    bool IsStatic,
    Microsoft.CodeAnalysis.ITypeSymbol TypeSymbol,
    Microsoft.CodeAnalysis.IMethodSymbol MethodSymbol
);

internal static class Emitter
{
    public static bool TryEmit(
        TargetMethodInfo[] methods,
        [NotNullWhen(true)] out string? code,
        out List<Diagnostic> diagnostics
    )
    {
        using var stringWriter = new StringWriter();
        using var writer = new IndentedTextWriter(stringWriter);
        diagnostics = [];

        INamedTypeSymbol parentTypeSymbol = methods[0].Symbol.ContainingType;

        writer.WriteMultiLine(
            """
            // <auto-generated/>
            #nullable enable
            """
        );
        writer.WriteLine();
        EmitOptionalNamespaceStart(writer, parentTypeSymbol);
        writer.WriteLine($"partial class {parentTypeSymbol.Name} : global::Darp.Utils.Messaging.IMessageSinkProvider");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteMultiLine(
            $"""
            [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
            {GetGeneratedVersionAttribute()}
            [global::System.Obsolete("This field is not intended to be used in use code. Use 'GetMessageSink'")]
            private ___MessageSink? ___lazyMessageSink;
            """
        );
        writer.WriteLineNoTabs("");
        writer.WriteMultiLine(
            $$"""
            /// <inheritdoc />
            {{GetGeneratedVersionAttribute()}}
            public global::Darp.Utils.Messaging.IMessageSink GetMessageSink()
            {
                return ___lazyMessageSink ??= new ___MessageSink(this);
            }
            """
        );
        writer.WriteLineNoTabs("");
        if (!TryEmitMessageSink(writer, parentTypeSymbol, methods, diagnostics))
        {
            code = null;
            return false;
        }
        writer.Indent--;
        writer.WriteLine("}");
        EmitOptionalNamespaceEnd(writer, parentTypeSymbol);
        code = stringWriter.ToString();
        return true;
    }

    private static bool TryEmitMessageSink(
        IndentedTextWriter writer,
        INamedTypeSymbol parentTypeSymbol,
        TargetMethodInfo[] methods,
        List<Diagnostic> diagnostics
    )
    {
        List<MethodInfo> messageTypes = [];
        var isValid = true;
        foreach (TargetMethodInfo targetMethodInfo in methods)
        {
            var isStatic = targetMethodInfo.Symbol.IsStatic;
            var isAny = false;
            if (targetMethodInfo.Symbol.Parameters.Length != 1)
            {
                isValid = false;
                diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidMethodParameters,
                        targetMethodInfo.SinkAttributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                    )
                );
                continue;
            }
            if (targetMethodInfo.Symbol.TypeParameters.Length > 0)
            {
                if (targetMethodInfo.Symbol.TypeParameters.Length > 1)
                {
                    isValid = false;
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidMethodTypeParameters,
                            targetMethodInfo.SinkAttributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                        )
                    );
                    continue;
                }
                var allowsRefStruct = targetMethodInfo.Symbol.TypeParameters[0].AllowsRefLikeType;
                if (!allowsRefStruct || targetMethodInfo.Symbol.TypeParameters[0].ConstraintTypes.Length > 0)
                {
                    isValid = false;
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidMethodTypeConstraint,
                            targetMethodInfo.SinkAttributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                        )
                    );
                    continue;
                }
                isAny = true;
            }
            messageTypes.Add((isAny, isStatic, targetMethodInfo.Symbol.Parameters[0].Type, targetMethodInfo.Symbol));
        }

        if (!isValid)
            return false;
        writer.WriteLine(GetGeneratedVersionAttribute());
        writer.WriteLine("private sealed class ___MessageSink");
        IEnumerable<string> methodParameterTypes = messageTypes
            .Where(x => !x.IsAny)
            .Select(x => x.TypeSymbol)
            .Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
            .Select(x =>
                $"global::Darp.Utils.Messaging.IMessageSink<{x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>"
            )
            .Concat(messageTypes.Any(x => x.IsAny) ? ["global::Darp.Utils.Messaging.IAnyMessageSink"] : []);
        writer.WriteLine($"    : {string.Join(", ", methodParameterTypes)}");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteMultiLine(
            $$"""
            private readonly {{parentTypeSymbol.Name}} _parent;

            public ___MessageSink({{parentTypeSymbol.Name}} parent)
            {
                _parent = parent;
            }
            
            """
        );
        foreach (
            IGrouping<ITypeSymbol, MethodInfo> methodGrouping in messageTypes
                .Where(x => !x.IsAny)
                .GroupBy(x => x.TypeSymbol, (IEqualityComparer<ITypeSymbol>)SymbolEqualityComparer.Default)
        )
        {
            EmitDefaultPublishMethod(writer, methodGrouping);
        }
        MethodInfo[] anyMethods = messageTypes.Where(x => x.IsAny).ToArray();
        if (anyMethods.Length > 0)
        {
            EmitAnyPublishMethod(writer, anyMethods);
        }
        writer.Indent--;
        writer.WriteLine("}");
        return isValid;
    }

    private static void EmitAnyPublishMethod(IndentedTextWriter writer, MethodInfo[] valueTuples)
    {
        writer.WriteLine(
            "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]"
        );
        writer.WriteLine("public void Publish<T>(in T message)");
        writer.WriteLineNoTabs("#if NET9_0_OR_GREATER");
        writer.WriteLine("    where T : allows ref struct");
        writer.WriteLineNoTabs("#endif");
        writer.WriteLine("{");
        writer.Indent++;
        foreach ((_, var isStatic, ITypeSymbol _, IMethodSymbol methodSymbol) in valueTuples)
        {
            if (isStatic)
                writer.WriteMultiLine($"{methodSymbol.Name}(message);");
            else
                writer.WriteMultiLine($"_parent.{methodSymbol.Name}(message);");
        }
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitDefaultPublishMethod(
        IndentedTextWriter writer,
        IGrouping<ITypeSymbol, MethodInfo> valueTuples
    )
    {
        var typeNameString = valueTuples.Key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        writer.WriteLine(
            "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]"
        );
        writer.WriteLine($"public void Publish(in {typeNameString} message)");
        writer.WriteLine("{");
        writer.Indent++;
        foreach ((_, var isStatic, ITypeSymbol _, IMethodSymbol methodSymbol) in valueTuples)
        {
            if (isStatic)
                writer.WriteMultiLine($"{methodSymbol.Name}(message);");
            else
                writer.WriteMultiLine($"_parent.{methodSymbol.Name}(message);");
        }
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitOptionalNamespaceStart(IndentedTextWriter writer, INamedTypeSymbol symbol)
    {
        var typeNamespace = symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        writer.WriteLine($"namespace {typeNamespace}");
        writer.WriteLine("{");
        writer.Indent++;
    }

    private static void EmitOptionalNamespaceEnd(IndentedTextWriter writer, INamedTypeSymbol symbol)
    {
        var typeNamespace = symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static string? GetNamespace(this ITypeSymbol symbol)
    {
        if (symbol.ContainingNamespace.IsGlobalNamespace)
            return null;
        var typeNamespace = symbol.ContainingNamespace.ToDisplayString();
        return string.IsNullOrWhiteSpace(typeNamespace) ? null : typeNamespace;
    }

    private static void WriteMultiLine(this IndentedTextWriter writer, string multiLineString)
    {
        foreach (var se in multiLineString.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (string.IsNullOrWhiteSpace(se))
                writer.WriteLineNoTabs("");
            else
                writer.WriteLine(se);
        }
    }

    private static string GetGeneratedVersionAttribute()
    {
        var generatorName = typeof(MessagingGenerator).Assembly.GetName().Name;
        Version generatorVersion = typeof(MessagingGenerator).Assembly.GetName().Version;
        return $"""[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{generatorName}", "{generatorVersion}")]""";
    }

    private static IEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null
    )
    {
        using IEnumerator<TSource> enumerator = source.GetEnumerator();

        if (enumerator.MoveNext())
        {
            var set = new HashSet<TKey>(comparer);
            do
            {
                TSource element = enumerator.Current;
                if (set.Add(keySelector(element)))
                {
                    yield return element;
                }
            } while (enumerator.MoveNext());
        }
    }
}
