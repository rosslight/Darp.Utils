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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

internal sealed class CSharpResxSourceGenerator : IIncrementalGenerator
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Standard practice for diagnosing source generator failures.")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> resourceFiles =
            context.AdditionalTextsProvider.Where(static file =>
                file.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase));
        IncrementalValueProvider<CompilationInformation> compilationInformation = context.CompilationProvider.Select(
            (compilation, _) =>
            {
                //var methodImplOptions = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesMethodImplOptions);
                //var hasAggressiveInlining = methodImplOptions?.MemberNames.Contains(nameof(MethodImplOptions.AggressiveInlining)) ?? false;
                //var hasNotNullIfNotNull = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute) is not null;

                return new CompilationInformation(
                    AssemblyName: compilation.AssemblyName,
                    HasAggressiveInlining: true); //hasAggressiveInlining);
            });
        IncrementalValuesProvider<ResourceInformation> resourceFilesToGenerateSource = resourceFiles
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Combine(compilationInformation)
            .SelectMany(static (resourceFileAndOptions, _) =>
            {
                ((AdditionalText resourceFile, AnalyzerConfigOptionsProvider optionsProvider),
                    CompilationInformation compilationInfo) = resourceFileAndOptions;
                AnalyzerConfigOptions options = optionsProvider.GetOptions(resourceFile);

                // Use the GenerateSource property if provided. Otherwise, the value of GenerateSource defaults to
                // true for resources without an explicit culture.
                var explicitGenerateSource = IsGenerateSource(options);
                if (explicitGenerateSource == false)
                {
                    // Source generation is explicitly disabled for this resource file
                    return Array.Empty<ResourceInformation>();
                }

                if (explicitGenerateSource != true)
                {
                    var implicitGenerateSource = !IsExplicitWithCulture(options);
                    if (!implicitGenerateSource)
                    {
                        // Source generation is disabled for this resource file
                        return Array.Empty<ResourceInformation>();
                    }
                }

                if (!optionsProvider.GlobalOptions.TryGetValue("build_property.RootNamespace",
                        out var rootNamespace))
                {
                    rootNamespace = compilationInfo.AssemblyName;
                }

                var resourceHintName = Path.GetFileNameWithoutExtension(resourceFile.Path);
                var resourceName = resourceHintName;
                if (options.TryGetValue("build_metadata.AdditionalFiles.RelativeDir", out var relativeDir))
                {
                    relativeDir = relativeDir
                        .Replace(Path.DirectorySeparatorChar, '.')
                        .Replace(Path.AltDirectorySeparatorChar, '.');
                    resourceName = relativeDir + resourceName;
                }

                options.TryGetValue("build_metadata.AdditionalFiles.ClassName", out var resourceClassName);

                if (!options.TryGetValue("build_metadata.AdditionalFiles.OmitGetResourceString",
                        out var omitGetResourceStringText)
                    || !bool.TryParse(omitGetResourceStringText, out var omitGetResourceString))
                {
                    omitGetResourceString = false;
                }

                if (!options.TryGetValue("build_metadata.AdditionalFiles.EmitFormatMethods",
                        out var emitFormatMethodsText)
                    || !bool.TryParse(emitFormatMethodsText, out var emitFormatMethods))
                {
                    emitFormatMethods = false;
                }

                if (!options.TryGetValue("build_metadata.AdditionalFiles.Public", out var publicText)
                    || !bool.TryParse(publicText, out var publicResource))
                {
                    publicResource = false;
                }

                return
                [
                    new ResourceInformation(
                        CompilationInformation: compilationInfo,
                        ResourceFile: resourceFile,
                        ResourceName: string.Join(".", rootNamespace, resourceName),
                        ResourceHintName: resourceHintName,
                        ResourceClassName: resourceClassName,
                        OmitGetResourceString: omitGetResourceString,
                        EmitFormatMethods: emitFormatMethods,
                        Public: publicResource),
                ];
            });
        IncrementalValueProvider<ImmutableDictionary<string, string>> renameMapping = resourceFilesToGenerateSource
            .Select(static (resourceFile, _) => (resourceFile.ResourceName, resourceFile.ResourceHintName))
            .Collect()
            .Select(static (resourceNames, _) =>
            {
                var names = new HashSet<string>();
                ImmutableDictionary<string, string> remappedNames = ImmutableDictionary<string, string>.Empty;
                foreach ((string resourceName, string resourceHintName) in resourceNames.OrderBy(x => x.ResourceName, StringComparer.Ordinal))
                {
                    for (var i = -1; ; i++)
                    {
                        if (i == -1)
                        {
                            if (names.Add(resourceHintName))
                                break;
                            continue;
                        }
                        var candidateName = resourceHintName + i;
                        if (!names.Add(candidateName))
                            continue;
                        remappedNames = remappedNames.Add(resourceName, candidateName);
                        break;
                    }
                }

                return remappedNames;
            })
            .WithComparer(ImmutableDictionaryEqualityComparer<string, string>.Instance);
        IncrementalValuesProvider<ResourceInformation> resourceFilesToGenerateSourceWithNames = resourceFilesToGenerateSource
            .Combine(renameMapping)
            .Select(static (resourceFileAndRenameMapping, _) =>
            {
                (ResourceInformation resourceFile, ImmutableDictionary<string, string> renameMapping) = resourceFileAndRenameMapping;
                if (renameMapping.TryGetValue(resourceFile.ResourceName, out var newHintName))
                {
                    return resourceFile with { ResourceHintName = newHintName };
                }

                return resourceFile;
            });

        context.RegisterSourceOutput(resourceFilesToGenerateSourceWithNames, static (context, resourceInformation) =>
        {
            try
            {
                var impl = new Impl(resourceInformation);
                if (impl.Execute(context.CancellationToken))
                {
                    context.AddSource(impl.OutputTextHintName, impl.OutputText);
                }
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var exceptionLines = ex.ToString().Split([Environment.NewLine], StringSplitOptions.None);
                var text = string.Join("", exceptionLines.Select(line => "#error " + line + Environment.NewLine));
                var errorText = SourceText.From(text, Encoding.UTF8, SourceHashAlgorithm.Sha256);
                context.AddSource($"{resourceInformation.ResourceHintName}.Error", errorText);
            }
        });
    }

    private static bool? IsGenerateSource(AnalyzerConfigOptions options)
    {
        if (!options.TryGetValue("build_metadata.AdditionalFiles.GenerateSource", out var generateSourceText)
            || !bool.TryParse(generateSourceText, out var generateSource))
        {
            // This resource did not explicitly set GenerateSource to true or false
            return null;
        }

        return generateSource;
    }

    private static bool IsExplicitWithCulture(AnalyzerConfigOptions options)
    {
        if (!options.TryGetValue("build_metadata.AdditionalFiles.WithCulture", out var withCultureText)
            || !bool.TryParse(withCultureText, out var withCulture))
        {
            // Assume the resource does not have a culture when there is no indication otherwise
            return false;
        }

        return withCulture;
    }
}
