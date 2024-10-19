// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.


namespace Darp.Utils.ResxSourceGenerator.Tests.Verifiers;

// Uncomment the following line to write expected files to disk
////#define WRITE_EXPECTED

#if WRITE_EXPECTED
#warning WRITE_EXPECTED is fine for local builds, but should not be merged to the main branch.
#endif

using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

public static partial class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        private readonly string _identifier;
        private readonly string? _testFile;
        private readonly string? _testMethod;

        public Test([CallerFilePath] string? testFile = null, [CallerMemberName] string? testMethod = null)
            : this(string.Empty, testFile, testMethod) { }

        public Test(
            string identifier,
            [CallerFilePath] string? testFile = null,
            [CallerMemberName] string? testMethod = null
        )
        {
            _identifier = identifier;
            _testFile = testFile;
            _testMethod = testMethod;

#if WRITE_EXPECTED
            TestBehaviors |= TestBehaviors.SkipGeneratedSourcesCheck;
#endif
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

        protected override string DefaultFileExt => "g.cs";

        private string ResourceName
        {
            get
            {
                if (string.IsNullOrEmpty(_identifier))
                    return _testMethod ?? "";

                return $"{_testMethod}_{_identifier}";
            }
        }

        protected override CompilationOptions CreateCompilationOptions()
        {
            CompilationOptions compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings)
            );
        }

        protected override ParseOptions CreateParseOptions() =>
            ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);

        protected override async Task<(
            Compilation compilation,
            ImmutableArray<Diagnostic> generatorDiagnostics
        )> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            var resourceDirectory = Path.Combine(Path.GetDirectoryName(_testFile)!, "Resources", ResourceName);

            (Compilation compilation, ImmutableArray<Diagnostic> generatorDiagnostics) =
                await base.GetProjectCompilationAsync(project, verifier, cancellationToken);
            var expectedNames = new HashSet<string>();
            foreach (SyntaxTree? tree in compilation.SyntaxTrees.Skip(project.DocumentIds.Count))
            {
                WriteTreeToDiskIfNecessary(tree, resourceDirectory);
                expectedNames.Add(Path.GetFileName(tree.FilePath));
            }

            var currentTestPrefix = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.{ResourceName}.";
            foreach (var name in GetType().Assembly.GetManifestResourceNames())
            {
                if (!name.StartsWith(currentTestPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!expectedNames.Contains(name.Substring(currentTestPrefix.Length)))
                {
                    throw new InvalidOperationException(
                        $"Unexpected test resource: {name.Substring(currentTestPrefix.Length)}"
                    );
                }
            }

            return (compilation, generatorDiagnostics);
        }

        public Test AddGeneratedSources()
        {
            var expectedPrefix = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.{ResourceName}.";
            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(expectedPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                using Stream resourceStream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException();
                using var reader = new StreamReader(
                    resourceStream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true
                );
                var name = resourceName.Substring(expectedPrefix.Length);
                var readData = reader.ReadToEnd();
                readData = readData.ReplaceLineEndings("\n");
                TestState.GeneratedSources.Add(
                    (
                        typeof(TSourceGenerator),
                        name,
                        SourceText.From(readData, Encoding.UTF8, SourceHashAlgorithm.Sha256)
                    )
                );
            }

            return this;
        }

        [Conditional("WRITE_EXPECTED")]
        private static void WriteTreeToDiskIfNecessary(SyntaxTree tree, string resourceDirectory)
        {
            if (tree.Encoding is null)
            {
                throw new ArgumentException("Syntax tree encoding was not specified");
            }

            var name = Path.GetFileName(tree.FilePath);
            var filePath = Path.Combine(resourceDirectory, name);
            Directory.CreateDirectory(resourceDirectory);
            File.WriteAllText(filePath, tree.GetText().ToString(), tree.Encoding);
        }
    }
}
