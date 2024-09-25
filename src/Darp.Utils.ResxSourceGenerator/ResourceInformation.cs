namespace Darp.Utils.ResxSourceGenerator;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;

/// <summary>
///
/// </summary>
/// <param name="CompilationInformation">Information about the compilation.</param>
/// <param name="ResourceFile">Resources (resx) file.</param>
/// <param name="Settings">Settings configured or their default value.</param>
internal readonly record struct ResourceInformation(
    CompilationInformation CompilationInformation,
    AdditionalText ResourceFile,
    ConfigurationSettings Settings,
    string ResourceAccessName,
    string? Namespace,
    string ClassName);

internal readonly record struct ResourceCollection(ResourceInformation BaseInformation,
    ImmutableDictionary<CultureInfo, AdditionalText> OtherLanguages,
    string FileHintName);

/// <param name="ResourceName">Name of the embedded resources to generate accessor class for.</param>
/// <param name="ResourceHintName">Unique identifying name for the generated resource file within the compilation. This will be the same as the last segment of <paramref name="ResourceName"/> (after the final <c>.</c>) except in the case of duplicates.</param>
/// <param name="ClassName">Optionally, a <c>namespace.type</c> name for the generated Resources accessor class. Defaults to <see cref="ResourceName"/> if unspecified.</param>
/// <param name="EmitFormatMethods">If set to <see langword="true"/>, the generated code will include <c>.FormatXYZ(...)</c> methods.</param>
/// <param name="Public">If set to <see langword="true"/>, the generated class will be declared <see langword="public"/>; otherwise, it will be declared <see langword="internal"/>.</param>
internal readonly record struct ConfigurationSettings(
    string? RootNamespace,
    string? RelativeDir,
    string? ClassName,
    bool EmitDebugInformation,
    bool EmitFormatMethods,
    bool Public);
