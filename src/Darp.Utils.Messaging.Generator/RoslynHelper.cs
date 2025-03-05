namespace Darp.Utils.Messaging.Generator;

using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;

internal static class RoslynHelper
{
    public static void EmitOptionalNamespaceStart(IndentedTextWriter writer, INamedTypeSymbol symbol)
    {
        var typeNamespace = symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        writer.WriteLine($"namespace {typeNamespace}");
        writer.WriteLine("{");
        writer.Indent++;
    }

    public static void EmitOptionalNamespaceEnd(IndentedTextWriter writer, INamedTypeSymbol symbol)
    {
        var typeNamespace = symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        writer.Indent--;
        writer.WriteLine("}");
    }

    public static string? GetNamespace(this ITypeSymbol symbol)
    {
        if (symbol.ContainingNamespace.IsGlobalNamespace)
            return null;
        var typeNamespace = symbol.ContainingNamespace.ToDisplayString();
        return string.IsNullOrWhiteSpace(typeNamespace) ? null : typeNamespace;
    }

    public static void WriteMultiLine(this IndentedTextWriter writer, string multiLineString)
    {
        foreach (var se in multiLineString.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (string.IsNullOrWhiteSpace(se))
                writer.WriteLineNoTabs("");
            else if (se.Trim().StartsWith("#", StringComparison.Ordinal))
                writer.WriteLineNoTabs(se);
            else
                writer.WriteLine(se);
        }
    }

    public static string GetGeneratedVersionAttribute()
    {
        var generatorName = typeof(MessagingGenerator).Assembly.GetName().Name;
        Version generatorVersion = typeof(MessagingGenerator).Assembly.GetName().Version;
        return $"""[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{generatorName}", "{generatorVersion}")]""";
    }

    public static string GetFileName(this INamedTypeSymbol typeSymbol)
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
