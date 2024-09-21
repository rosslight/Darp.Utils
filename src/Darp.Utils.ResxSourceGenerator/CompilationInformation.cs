namespace Darp.Utils.ResxSourceGenerator;

/// <summary>
///
/// </summary>
/// <param name="AssemblyName"></param>
/// <param name="CodeLanguage">Language of source file to generate. Supported languages: CSharp, VisualBasic.</param>
/// <param name="SupportsNullable"></param>
internal sealed record CompilationInformation(
    string? AssemblyName,
    string CodeLanguage,
    bool SupportsNullable,
    bool HasAggressiveInlining,
    bool HasNotNullIfNotNull);