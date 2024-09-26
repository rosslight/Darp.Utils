namespace Darp.Utils.ResxSourceGenerator;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;

/// <param name="CompilationInformation">Information about the compilation.</param>
/// <param name="ResourceFile">Resources (resx) file.</param>
/// <param name="Settings">Settings configured or their default value.</param>
/// <param name="ResourceFileName"> The file name of the resource file ("Resources", for First/Resources.resx) </param>
/// <param name="ResourceName"> The name of the resource file ("First.Resources", for First/Resources.resx) </param>
/// <param name="Namespace"> The namespace of the resource file ("TestAssembly.First", for First/Resources.resx in assembly TestAssembly) </param>
/// <param name="ClassName"> The className of the resource file ("Resources", for First/Resources.resx) </param>
internal readonly record struct ResourceInformation(
    CompilationInformation CompilationInformation,
    AdditionalText ResourceFile,
    ConfigurationSettings Settings,
    string ResourceFileName,
    string ResourceName,
    string? Namespace,
    string ClassName);

/// <param name="BaseInformation"> The resourceInformation about the default resource file </param>
/// <param name="OtherLanguages"> Additional languages in respect to the default resource file </param>
/// <param name="FileHintName"> The file name of the generated file (<see cref="ResourceInformation.ResourceFileName"/> or adjusted in case of duplication ("Resources.g.cs") </param>
internal readonly record struct ResourceCollection(ResourceInformation BaseInformation,
    ImmutableDictionary<CultureInfo, AdditionalText> OtherLanguages,
    string FileHintName);

/// <param name="RootNamespace">Optionally, a root namespace. Defaults to the Assembly name</param>
/// <param name="RelativeDir">Optionally, a default directory</param>
/// <param name="ClassName">Optionally, a <c>namespace.type</c> name for the generated Resources accessor class. </param>
/// <param name="EmitDebugInformation">If set to <see langword="true"/>, the generated code will contain a section with debugging information</param>
/// <param name="EmitFormatMethods">If set to <see langword="true"/>, the generated code will include <c>.FormatXYZ(...)</c> methods.</param>
/// <param name="Public">If set to <see langword="true"/>, the generated class will be declared <see langword="public"/>; otherwise, it will be declared <see langword="internal"/>.</param>
internal readonly record struct ConfigurationSettings(
    string? RootNamespace,
    string? RelativeDir,
    string? ClassName,
    bool EmitDebugInformation,
    bool EmitFormatMethods,
    bool Public);

internal readonly record struct CompilationInformation(string? AssemblyName);
