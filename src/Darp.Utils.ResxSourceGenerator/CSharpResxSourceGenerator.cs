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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Resource = (ResourceInformation ResourceInformation, string ResourcePathName);

[Generator]
internal sealed class CSharpResxSourceGenerator : IIncrementalGenerator
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Standard practice for diagnosing source generator failures.")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // if (!Debugger.IsAttached)
        //     Debugger.Launch();
        IncrementalValuesProvider<AdditionalText> resourceFiles = context
            .AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase));
        IncrementalValueProvider<CompilationInformation> compilationInformation = context.CompilationProvider.Select(
            (compilation, _) =>
            {
                //var methodImplOptions = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesMethodImplOptions);
                //var hasAggressiveInlining = methodImplOptions?.MemberNames.Contains(nameof(MethodImplOptions.AggressiveInlining)) ?? false;
                //var hasNotNullIfNotNull = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute) is not null;

                return new CompilationInformation(
                    AssemblyName: compilation.AssemblyName); //hasAggressiveInlining);
            });
        IncrementalValuesProvider<Resource> resourceFilesToGenerateSource = resourceFiles
            .Combine(context.AnalyzerConfigOptionsProvider.Combine(compilationInformation))
            .SelectMany(static (resourceFileAndOptions, _) =>
            {
                (AdditionalText resourceFile, (AnalyzerConfigOptionsProvider optionsProvider, CompilationInformation compilationInfo)) = resourceFileAndOptions;
                AnalyzerConfigOptions globalOptions = optionsProvider.GlobalOptions;
                AnalyzerConfigOptions options = optionsProvider.GetOptions(resourceFile);

                if (!(options.GetBoolValue("build_metadata.AdditionalFiles.GenerateSource") ?? true))
                {
                    // Source generation is explicitly disabled for this resource file
                    return Array.Empty<(ResourceInformation, string)>();
                }

                var rootNamespace = globalOptions.GetValue("build_property.RootNamespace")
                                    ?? compilationInfo.AssemblyName;

                var relativeDir = options.GetValue("build_metadata.AdditionalFiles.RelativeDir");
                var className = options.GetValue("build_metadata.AdditionalFiles.ClassName");
                var emitFormatMethods = options.GetBoolValue("build_metadata.AdditionalFiles.EmitFormatMethods")
                                        ?? false;
                var publicResource = options.GetBoolValue("build_metadata.AdditionalFiles.Public")
                                     ?? false;

                var info = new ResourceInformation(
                    CompilationInformation: compilationInfo,
                    ResourceFile: resourceFile,
                    Settings: new ConfigurationSettings(
                        RootNamespace: rootNamespace,
                        RelativeDir: relativeDir,
                        ClassName: className,
                        EmitFormatMethods: emitFormatMethods,
                        Public: publicResource));
                return [(info, Path.GetFileNameWithoutExtension(resourceFile.Path))];
            });
        IncrementalValueProvider<ImmutableDictionary<ResourceInformation, string>> renameMapping = resourceFilesToGenerateSource
            .Collect()
            .Select(static (resource, _) =>
            {
                var names = new HashSet<string>();
                ImmutableDictionary<ResourceInformation, string> remappedNames = ImmutableDictionary<ResourceInformation, string>.Empty;
                foreach ((ResourceInformation resourceInformation, var resourceName) in resource
                             .OrderBy(x => x.ResourcePathName, StringComparer.Ordinal))
                {
                    for (var i = -1;; i++)
                    {
                        if (i == -1)
                        {
                            if (names.Add(resourceName))
                                break;
                        }
                        else
                        {
                            var candidateName = i.ToString(CultureInfo.InvariantCulture);
                            if (!names.Add(candidateName)) continue;
                            remappedNames = remappedNames.Add(resourceInformation, candidateName);
                            break;
                        }
                    }
                }

                return remappedNames;
            })
            .WithComparer(ImmutableDictionaryEqualityComparer<ResourceInformation, string>.Instance);
        IncrementalValueProvider<ImmutableArray<Resource>> allFilesSource = resourceFilesToGenerateSource
            .Collect();

        IncrementalValuesProvider<ResourceCollection> resourceFilesToGenerateSourceWithNames = resourceFilesToGenerateSource
            .Combine(renameMapping)
            .Combine(allFilesSource)
            .Where(x =>
            {
                ((Resource resource, _), ImmutableArray<Resource> allFiles) = x;
                return !BuildHelper.IsChildFile(resource.ResourcePathName, allFiles.Select(r => r.ResourcePathName));
            })
            .Select(static (resourceFileAndRenameMapping, _) =>
            {
                var ((resource,  mappings), allFiles) = resourceFileAndRenameMapping;
                var fileHintName = mappings.TryGetValue(resource.ResourceInformation, out var fileMapping)
                    ? $"{resource.ResourcePathName}{fileMapping}.Designer.g.cs"
                    : $"{resource.ResourcePathName}.Designer.g.cs";
                return new ResourceCollection(resource.ResourceInformation,
                    allFiles
                        .Where(x => x != resource)
                        .Where(x => BuildHelper.IsChildFile(x.ResourcePathName, allFiles.Select(r => r.ResourcePathName)))
                        .Select(x => x.Item1.ResourceFile)
                        .ToImmutableArray(),
                    fileHintName);
            });

        context.RegisterSourceOutput(resourceFilesToGenerateSourceWithNames, static (context, resourceInformation) =>
        {
            try
            {
                if (BuildHelper.TryGenerateSource(resourceInformation,
                        out IEnumerable<Diagnostic> diagnostics,
                        out var fileName,
                        out var sourceText,
                        context.CancellationToken))
                {
                    context.AddSource(fileName, SourceText.From(sourceText, Encoding.UTF8, SourceHashAlgorithm.Sha256));
                }
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var exceptionLines = ex.ToString().Split(['\n'], StringSplitOptions.None);
                var text = string.Join("", exceptionLines.Select(line => "#error " + line + "\n"));
                var errorText = SourceText.From(text, Encoding.UTF8, SourceHashAlgorithm.Sha256);
                context.AddSource($"Error", errorText);
            }
        });
    }
}
