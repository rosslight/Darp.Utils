namespace Darp.Utils.ResxSourceGenerator;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;

internal readonly record struct ResourceInformation
{
    /// <summary>Information about the compilation.</summary>
    public required CompilationInformation CompilationInformation { get; init; }

    /// <summary>Resources (resx) file.</summary>
    public required AdditionalText ResourceFile { get; init; }

    /// <summary>Settings configured or their default value.</summary>
    public required ConfigurationSettings Settings { get; init; }

    /// <summary> The file name of the resource file ("Resources", for First/Resources.resx) </summary>
    public required string ResourceFileName { get; init; }

    /// <summary> The name of the resource file ("First.Resources", for First/Resources.resx) </summary>
    public required string ResourceName { get; init; }

    /// <summary> The namespace of the resource file ("TestAssembly.First", for First/Resources.resx in assembly TestAssembly) </summary>
    public required string? Namespace { get; init; }

    /// <summary> The className of the resource file ("Resources", for First/Resources.resx) </summary>
    public required string ClassName { get; init; }
}

internal readonly record struct ResourceCollection
{
    /// <summary> The resourceInformation about the default resource file </summary>
    public required ResourceInformation BaseInformation { get; init; }

    /// <summary> Additional languages in respect to the default resource file </summary>
    public required ImmutableDictionary<CultureInfo, AdditionalText> OtherLanguages { get; init; }

    /// <summary> The file name of the generated file (<see cref="ResourceInformation.ResourceFileName"/> or adjusted in case of duplication ("Resources.g.cs") </summary>
    public required string FileHintName { get; init; }
}

internal readonly record struct ConfigurationSettings
{
    /// <summary>Optionally, a root namespace. Defaults to the Assembly name</summary>
    public required string? RootNamespace { get; init; }

    /// <summary>Optionally, a default directory</summary>
    public required string? RelativeDir { get; init; }

    /// <summary>Optionally, a <c>namespace.type</c> name for the generated Resources accessor class. </summary>
    public required string? ClassName { get; init; }

    /// <summary>If set to <see langword="true"/>, the generated code will contain a section with debugging information</summary>
    public required bool EmitDebugInformation { get; init; }

    /// <summary>If set to <see langword="true"/>, the generated code will include <c>.FormatXYZ(...)</c> methods.</summary>
    public required bool EmitFormatMethods { get; init; }

    /// <summary>If set to <see langword="true"/>, the generated class will be declared <see langword="public"/>; otherwise, it will be declared <see langword="internal"/>.</summary>
    public required bool Public { get; init; }
}

internal readonly record struct CompilationInformation
{
    public required string? AssemblyName { get; init; }
}
