// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.
// https://github.com/dotnet/roslyn-analyzers/blame/f4c8475010cbc3d5956c99c1f2c2d49c03c5871b/src/Microsoft.CodeAnalysis.ResxSourceGenerator/Microsoft.CodeAnalysis.ResxSourceGenerator/AbstractResxGenerator.cs


// #pragma warning disable IDE0010 // Add missing cases (noise)
// #pragma warning disable IDE0057 // Use range operator (incorrectly reported when Range is not defined)
// #pragma warning disable IDE0058 // Expression value is never used (not sure why this is enabled)
// #pragma warning disable IDE0066 // Convert switch statement to expression (not always better)

namespace Darp.Utils.ResxSourceGenerator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

[Generator]
internal sealed class CSharpResxSourceGenerator : IIncrementalGenerator
{
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Standard practice for diagnosing source generator failures."
    )]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> resourceFiles =
            context.AdditionalTextsProvider.Where(static file =>
                file.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase)
            );
        IncrementalValueProvider<CompilationInformation> compilationInformation =
            context.CompilationProvider.Select(
                static (compilation, _) =>
                    new CompilationInformation { AssemblyName = compilation.AssemblyName }
            );
        IncrementalValuesProvider<ResourceInformation> resourceFilesToGenerateSource = resourceFiles
            .Combine(context.AnalyzerConfigOptionsProvider.Combine(compilationInformation))
            .SelectMany(
                static (values, _) =>
                {
                    (
                        AdditionalText resourceFile,
                        (
                            AnalyzerConfigOptionsProvider optionsProvider,
                            CompilationInformation compilationInfo
                        )
                    ) = values;
                    return CreateResourceInformation(
                        optionsProvider,
                        resourceFile,
                        compilationInfo
                    );
                }
            );
        IncrementalValueProvider<ImmutableArray<ResourceInformation>> allFilesSource =
            resourceFilesToGenerateSource.Collect();
        IncrementalValueProvider<ImmutableDictionary<ResourceInformation, string>> renameMapping =
            allFilesSource
                .Select(static (values, _) => CreateNamePrefixMapping(values))
                .WithComparer(
                    ImmutableDictionaryEqualityComparer<ResourceInformation, string>.Instance
                );
        IncrementalValuesProvider<ResourceCollection> resourceFilesToGenerateSourceWithNames =
            resourceFilesToGenerateSource
                .Combine(renameMapping)
                .Combine(allFilesSource)
                .Where(static values =>
                {
                    (
                        (ResourceInformation resource, _),
                        ImmutableArray<ResourceInformation> allFiles
                    ) = values;
                    return !BuildHelper.IsChildFile(
                        resource.ResourceName,
                        allFiles.Select(r => r.ResourceName),
                        out _
                    );
                })
                .Select(
                    static (values, _) =>
                    {
                        (
                            (
                                ResourceInformation resource,
                                ImmutableDictionary<ResourceInformation, string>? mappings
                            ),
                            ImmutableArray<ResourceInformation> allFiles
                        ) = values;
                        return CreateResourceCollection(mappings, resource, allFiles);
                    }
                );

        context.RegisterSourceOutput(
            resourceFilesToGenerateSourceWithNames,
            static (context, resourceInformation) =>
            {
                try
                {
                    List<Diagnostic> diagnostics = [];
                    if (
                        BuildHelper.TryGenerateSource(
                            resourceInformation,
                            diagnostics,
                            out var sourceText,
                            context.CancellationToken
                        )
                    )
                    {
                        context.AddSource(
                            resourceInformation.FileHintName,
                            SourceText.From(sourceText, Encoding.UTF8, SourceHashAlgorithm.Sha256)
                        );
                    }
                    foreach (Diagnostic diagnostic in diagnostics)
                    {
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch (OperationCanceledException)
                    when (context.CancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var exceptionLines = ex.ToString().Split(['\n'], StringSplitOptions.None);
                    var text = string.Join(
                        "",
                        exceptionLines.Select(line => "#error " + line + "\n")
                    );
                    var errorText = SourceText.From(
                        text,
                        Encoding.UTF8,
                        SourceHashAlgorithm.Sha256
                    );
                    context.AddSource($"Error", errorText);
                }
            }
        );
    }

    private static ResourceInformation[] CreateResourceInformation(
        AnalyzerConfigOptionsProvider optionsProvider,
        AdditionalText resourceFile,
        CompilationInformation compilationInfo
    )
    {
        AnalyzerConfigOptions globalOptions = optionsProvider.GlobalOptions;
        AnalyzerConfigOptions options = optionsProvider.GetOptions(resourceFile);

        if (!(options.GetBoolValue("build_metadata.EmbeddedResource.GenerateSource") ?? true))
        {
            // Source generation is explicitly disabled for this resource file
            return [];
        }

        var rootNamespace =
            globalOptions.GetValue("build_property.RootNamespace") ?? compilationInfo.AssemblyName;
        var emitDebugInformation =
            globalOptions.GetBoolValue("build_property.ResxSourceGenerator_EmitDebugInformation")
            ?? false;

        var relativeDir = options.GetValue("build_metadata.EmbeddedResource.RelativeDir");
        var className = options.GetValue("build_metadata.EmbeddedResource.ClassName");
        var emitFormatMethods =
            options.GetBoolValue("build_metadata.EmbeddedResource.EmitFormatMethods") ?? false;
        var publicResource =
            options.GetBoolValue("build_metadata.EmbeddedResource.Public") ?? false;

        var resourcePathName = Path.GetFileNameWithoutExtension(resourceFile.Path);
        var computedResourceName = resourcePathName;
        if (relativeDir is not null)
        {
            var replacedRelativeDir = relativeDir
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.');
            computedResourceName = replacedRelativeDir + computedResourceName;
        }
        var resourceAccessName =
            className is null || string.IsNullOrEmpty(className)
                ? string.Join(".", rootNamespace, computedResourceName)
                : className;
        BuildHelper.SplitName(
            resourceAccessName,
            out var computedNamespaceName,
            out var computedClassName
        );

        return
        [
            new ResourceInformation
            {
                CompilationInformation = compilationInfo,
                ResourceFile = resourceFile,
                Settings = new ConfigurationSettings
                {
                    RootNamespace = rootNamespace,
                    RelativeDir = relativeDir,
                    ClassName = className,
                    EmitDebugInformation = emitDebugInformation,
                    EmitFormatMethods = emitFormatMethods,
                    Public = publicResource,
                },
                ResourceFileName = resourcePathName,
                ResourceName = string.Join(".", rootNamespace, computedResourceName),
                Namespace = computedNamespaceName,
                ClassName = computedClassName,
            },
        ];
    }

    private static ImmutableDictionary<ResourceInformation, string> CreateNamePrefixMapping(
        ImmutableArray<ResourceInformation> resource
    )
    {
        var names = new HashSet<string>();
        ImmutableDictionary<ResourceInformation, string> remappedNames = ImmutableDictionary<
            ResourceInformation,
            string
        >.Empty;
        foreach (
            ResourceInformation resourceInformation in resource.OrderBy(
                x => x.ResourceName,
                StringComparer.Ordinal
            )
        )
        {
            for (var i = -1; ; i++)
            {
                if (i == -1)
                {
                    if (names.Add(resourceInformation.ResourceFileName))
                        break;
                }
                else
                {
                    var candidateName = i.ToString(CultureInfo.InvariantCulture);
                    if (!names.Add(candidateName))
                        continue;
                    remappedNames = remappedNames.Add(resourceInformation, candidateName);
                    break;
                }
            }
        }

        return remappedNames;
    }

    private static ResourceCollection CreateResourceCollection(
        ImmutableDictionary<ResourceInformation, string> mappings,
        ResourceInformation resource,
        ImmutableArray<ResourceInformation> allFiles
    )
    {
        var fileHintName = mappings.TryGetValue(resource, out var fileMapping)
            ? $"{resource.ResourceFileName}{fileMapping}.Designer.g.cs"
            : $"{resource.ResourceFileName}.Designer.g.cs";
        return new ResourceCollection
        {
            BaseInformation = resource,
            OtherLanguages = allFiles
                .Where(x => x != resource)
                .Select(x =>
                {
                    var isChildFile = BuildHelper.IsChildFile(
                        x.ResourceName,
                        allFiles.Select(r => r.ResourceName),
                        out CultureInfo? cultureInfo
                    );
                    return !isChildFile
                        ? ((CultureInfo?, AdditionalText)?)null
                        : (cultureInfo, x.ResourceFile);
                })
                .Where(x => x is not null)
                .ToImmutableDictionary(x => x!.Value.Item1!, x => x!.Value.Item2),
            FileHintName = fileHintName,
        };
    }
}
