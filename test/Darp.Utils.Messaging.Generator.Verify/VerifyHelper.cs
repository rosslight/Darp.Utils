namespace Darp.Utils.Messaging.Generator.Verify;

using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Darp.Utils.Messaging.Generator;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static partial class VerifyHelper
{
    public static SettingsTask VerifyMessagingGenerator(
        string source,
        [CallerFilePath] string? callerFilePath = null,
        LanguageVersion version = LanguageVersion.CSharp13
    )
    {
        var fileName =
            Path.GetFileNameWithoutExtension(callerFilePath) ?? throw new ArgumentNullException(nameof(callerFilePath));
        return VerifyGenerator<MessagingGenerator>([source], "DBO0", fileName, version)
            .AddReferenceAssemblyMarker<MessagingGenerator>()
            .ScrubGeneratedCodeAttribute();
    }

    [GeneratedRegex("""GeneratedCodeAttribute\("[^"\n]+",\s*"(?<version>\d+\.\d+\.\d+\.\d+)"\)""")]
    private static partial Regex GetGeneratedCodeRegex();

    /// <summary>
    /// This functions ensures that the assembly is referenced and <see cref="AppDomain.GetAssemblies()"/> of the <see cref="AppDomain.CurrentDomain"/> contains this assembly
    /// </summary>
    /// <param name="settingsTask"> The task </param>
    /// <typeparam name="TMarker"> The type of an object of the assembly </typeparam>
    /// <returns> The task </returns>
    public static SettingsTask AddReferenceAssemblyMarker<TMarker>(this SettingsTask settingsTask) => settingsTask;

    public static SettingsTask ScrubGeneratedCodeAttribute(
        this SettingsTask settingsTask,
        string scrubbedVersionName = "GeneratorVersion"
    )
    {
        return settingsTask.ScrubLinesWithReplace(line =>
        {
            Regex regex = GetGeneratedCodeRegex();
            return regex.Replace(
                line,
                match =>
                {
                    var versionToReplace = match.Groups["version"].Value;
                    return match.Value.Replace(versionToReplace, scrubbedVersionName);
                }
            );
        });
    }

    private static SettingsTask VerifyGenerator<TGenerator>(
        string[] sources,
        string? allowedDiagnosticCode,
        string directory,
        LanguageVersion languageVersion = LanguageVersion.Default,
        NullableContextOptions nullableContextOptions = NullableContextOptions.Enable
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);
        SyntaxTree[] syntaxTrees = sources.Select(x => CSharpSyntaxTree.ParseText(x, parseOptions)).ToArray();

        // Get all references of the currently loaded assembly
        PortableExecutableReference[] references = AppDomain
            .CurrentDomain.GetAssemblies() // Get currently loaded assemblies
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: Assembly.GetExecutingAssembly().FullName,
            syntaxTrees: syntaxTrees,
            references: references,
            new CSharpCompilationOptions(OutputKind.NetModule, nullableContextOptions: nullableContextOptions)
        );

        // Assert that there are no compilation errors (except for CS5001 which informs about the missing program entry)
        Assert.DoesNotContain(
            compilation.GetDiagnostics(),
            x => x.Id is not "CS5001" && (x.Severity > DiagnosticSeverity.Warning || x.IsWarningAsError)
        );

        var generator = new TGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            parseOptions: parseOptions
        );
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Compilation newCompilation,
            out ImmutableArray<Diagnostic> diagnostics
        );
        // Assert that there are no compilation errors (except for CS5001 which informs about the missing program entry)
        newCompilation
            .GetDiagnostics()
            .Should()
            .NotContain(
                x => IsDiagnosticInvalid(allowedDiagnosticCode, x),
                "generated sources throw:\n{0}",
                string.Join("\n", driver.GetRunResult().GeneratedTrees.ToReadableString())
            );
        return Verifier.Verify(driver).UseDirectory(Path.Join("Snapshots", directory));
    }

    private static string ToReadableString(this ImmutableArray<SyntaxTree> syntaxTrees)
    {
        return string.Join("\n", syntaxTrees.SelectMany(x => x.GetText().Lines).Select((x, i) => $"{i + 1, 4} {x}"));
    }

    private static bool IsDiagnosticInvalid(string? allowedDiagnosticCode, Diagnostic x)
    {
        return x.Id is not "CS5001"
            && (allowedDiagnosticCode is null || !x.Id.StartsWith(allowedDiagnosticCode, StringComparison.Ordinal))
            && (x.Severity > DiagnosticSeverity.Warning || x.IsWarningAsError);
    }

    // Do not delete! This method is required for loading Darp.Utils.Messaging
    [MessageSink]
    private static void _() { }
}
