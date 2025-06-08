namespace Darp.Utils.CodeMirror;

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using MirrorSharp;

/// <summary> Extensions for <see cref="MirrorSharpCSharpOptions"/> </summary>
public static class MirrorSharpCSharpOptionsExtensions
{
    /// <summary> Add usings for the compilation </summary>
    /// <param name="options"> The <see cref="MirrorSharpCSharpOptions"/> options </param>
    /// <param name="usings"> The namespaces to add </param>
    /// <returns> The original <paramref name="options"/> </returns>
    public static MirrorSharpCSharpOptions AddUsings(
        this MirrorSharpCSharpOptions options,
        params IEnumerable<string> usings
    )
    {
        options.CompilationOptions = options.CompilationOptions.WithUsings(usings);
        return options;
    }

    /// <summary> Adds the metadata references </summary>
    /// <param name="options"> The <see cref="MirrorSharpCSharpOptions"/> options </param>
    /// <param name="assemblies"> The assemblies to add as a <see cref="MetadataReference"/> </param>
    /// <returns> The original <paramref name="options"/> </returns>
    public static MirrorSharpCSharpOptions AddMetadataReferencesFromAssembly(
        this MirrorSharpCSharpOptions options,
        params IEnumerable<Assembly> assemblies
    )
    {
        IEnumerable<PortableExecutableReference> references = assemblies.Select(x =>
            MetadataReference.CreateFromFile(x.Location)
        );
        options.MetadataReferences = options.MetadataReferences.AddRange(references);
        return options;
    }

    /// <summary> Sets the metadata references </summary>
    /// <param name="options"> The <see cref="MirrorSharpCSharpOptions"/> options </param>
    /// <param name="assemblies"> The assemblies to add as a <see cref="MetadataReference"/> </param>
    /// <returns> The original <paramref name="options"/> </returns>
    public static MirrorSharpCSharpOptions SetMetadataReferencesFromAssembly(
        this MirrorSharpCSharpOptions options,
        params IEnumerable<Assembly> assemblies
    )
    {
        options.MetadataReferences = ImmutableList<MetadataReference>.Empty;
        return options.AddMetadataReferencesFromAssembly(assemblies);
    }
}
