namespace Darp.Utils.ResxSourceGenerator;

/// <summary>
///
/// </summary>
/// <param name="AssemblyName"></param>
internal sealed record CompilationInformation(
    string? AssemblyName,
    bool HasAggressiveInlining);
