namespace Darp.Utils.ResxSourceGenerator;

using Microsoft.CodeAnalysis;

/// <summary>
///
/// </summary>
/// <param name="CompilationInformation">Information about the compilation.</param>
/// <param name="ResourceFile">Resources (resx) file.</param>
/// <param name="ResourceName">Name of the embedded resources to generate accessor class for.</param>
/// <param name="ResourceHintName">Unique identifying name for the generated resource file within the compilation. This will be the same as the last segment of <paramref name="ResourceName"/> (after the final <c>.</c>) except in the case of duplicates.</param>
/// <param name="ResourceClassName">Optionally, a <c>namespace.type</c> name for the generated Resources accessor class. Defaults to <see cref="ResourceName"/> if unspecified.</param>
/// <param name="OmitGetResourceString">If set to <see langword="true"/>, the <c>GetResourceString</c> method is not included in the generated class and must be specified in a separate source file.</param>
/// <param name="AsConstants">If set to <see langword="true"/>, emits constant key strings instead of properties that retrieve values.</param>
/// <param name="IncludeDefaultValues">If set to <see langword="true"/>, calls to <c>GetResourceString</c> receive a default resource string value.</param>
/// <param name="EmitFormatMethods">If set to <see langword="true"/>, the generated code will include <c>.FormatXYZ(...)</c> methods.</param>
/// <param name="Public">If set to <see langword="true"/>, the generated class will be declared <see langword="public"/>; otherwise, it will be declared <see langword="internal"/>.</param>
internal sealed record ResourceInformation(
    CompilationInformation CompilationInformation,
    AdditionalText ResourceFile,
    string ResourceName,
    string ResourceHintName,
    string? ResourceClassName,
    bool OmitGetResourceString,
    bool AsConstants,
    bool IncludeDefaultValues,
    bool EmitFormatMethods,
    bool Public);