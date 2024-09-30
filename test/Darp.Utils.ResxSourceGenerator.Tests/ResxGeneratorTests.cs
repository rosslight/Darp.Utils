// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Darp.Utils.ResxSourceGenerator.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using static ResxConstants;
using CSharpLanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VerifyCS = Verifiers.CSharpSourceGeneratorVerifier<CSharpResxSourceGenerator>;

public class ResxGeneratorTests
{
    [Theory]
    [InlineData(CSharpLanguageVersion.CSharp7, Skip = "Not supported (Missing nullable features)")]
    [InlineData(CSharpLanguageVersion.CSharp8)]
    [InlineData(CSharpLanguageVersion.CSharp9)]
    [InlineData(CSharpLanguageVersion.CSharp10)]
    [InlineData(CSharpLanguageVersion.CSharp11)]
    [InlineData(CSharpLanguageVersion.CSharp12)]
    public async Task SingleString_DefaultAsync(CSharpLanguageVersion languageVersion)
    {
        await new VerifyCS.Test
        {
            LanguageVersion = languageVersion,
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxValueDocument) },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Fact]
    public async Task TwoResourcesSameName_DefaultAsync()
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                AdditionalFiles =
                {
                    ("/0/First/Resources.resx", ResxValueDocument),
                    ("/0/Second/Resources.resx", ResxValueDocument),
                },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", @"
is_global = true

build_property.RootNamespace = TestProject

[/0/First/Resources.resx]
build_metadata.EmbeddedResource.RelativeDir = First/

[/0/Second/Resources.resx]
build_metadata.EmbeddedResource.RelativeDir = Second/
"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Theory]
    [InlineData("", Skip = "Empty root namespaces are not supported")]
    [InlineData("NS")]
    [InlineData("NS1.NS2")]
    public async Task SingleString_RootNamespaceAsync(string rootNamespace)
    {
        await new VerifyCS.Test(identifier: rootNamespace)
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxValueDocument) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

build_property.RootNamespace = {rootNamespace}
"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("NS")]
    [InlineData("NS1.NS2")]
    public async Task SingleString_RelativeDirAsync(string relativeDir)
    {
        await new VerifyCS.Test(identifier: relativeDir)
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxValueDocument) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

[/0/Resources.resx]
build_metadata.EmbeddedResource.RelativeDir = {relativeDir}
"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("NS")]
    [InlineData("NS1.NS2")]
    public async Task SingleString_ClassNameAsync(string className)
    {
        await new VerifyCS.Test(identifier: className)
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxValueDocument) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $"""
is_global = true

[/0/Resources.resx]
build_metadata.EmbeddedResource.ClassName = {className}
"""),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Theory]
    [InlineData("0", "value {0}")]
    [InlineData("replacement", "value {replacement}")]
    [InlineData("x", "value {x}")]
    [InlineData("0_1_2", "value {0} {1} {2}")]
    public async Task SingleString_EmitFormatMethodsAsync(string identifier, string value)
    {
        var code = ResxDocument("Name", value);

        await new VerifyCS.Test(identifier: identifier)
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", code) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", """
is_global = true

[/0/Resources.resx]
build_metadata.EmbeddedResource.EmitFormatMethods = true
"""),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Fact]
    public async Task SingleString_DifferentLanguagesAsync()
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                AdditionalFiles =
                {
                    ("/0/Resources.resx", ResxDocument("Name", "value")),
                    ("/0/Resources.de-DE.resx", ResxDocument("Name", "DE: value")),
                    ("/0/Resources.fr.resx", ResxDocument("Name", "FR: value")),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Fact]
    public async Task SingleString_PublicAsync()
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxValueDocument) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", """
is_global = true

[/0/Resources.resx]
build_metadata.EmbeddedResource.Public = true
"""),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Fact]
    public async Task SingleString_DebugInformationAsync()
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxValueDocument) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", """
is_global = true

build_property.ResxSourceGenerator_EmitDebugInformation = true
"""),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Fact]
    public async Task DarpResX001_RaiseOnEmptyDocument()
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxEmptyDocument) },
                Sources = { "" },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult("DarpResX001", DiagnosticSeverity.Warning)
                        .WithLocation("/0/Resources.resx", default),
                },
            },
        }.RunAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task DarpResX002_RaiseOnInvalidKey(string key)
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { "" },
                AdditionalFiles = { ("/0/Resources.resx", ResxDocument(key, "value")) },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult("DarpResX001", DiagnosticSeverity.Warning)
                        .WithLocation("/0/Resources.resx", default),
                    new DiagnosticResult("DarpResX002", DiagnosticSeverity.Warning)
                        .WithSpan("/0/Resources.resx", 61, 2, 61, 2 + key.Length)
                        .WithArguments(key),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task DarpResX003_RaiseOnMissingValue()
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { "" },
                AdditionalFiles = { ("/0/Resources.resx", ResxMissingValueDocument) },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult("DarpResX001", DiagnosticSeverity.Warning)
                        .WithLocation("/0/Resources.resx", default),
                    new DiagnosticResult("DarpResX003", DiagnosticSeverity.Warning)
                        .WithSpan("/0/Resources.resx", 61, 9, 61, 13)
                        .WithArguments("Name"),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task DarpResX004_RaiseOnDuplicateValue()
    {
        await new VerifyCS.Test(testMethod: nameof(SingleString_DefaultAsync))
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", ResxDocumentWithValues(
                    [
                        ("Name", "value"),
                        ("Name", "Value2"),
                    ])),
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult("DarpResX004", DiagnosticSeverity.Warning)
                        .WithSpan("/0/Resources.resx", 62, 14, 62, 18)
                        .WithArguments("Name"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Fact]
    public async Task DarpResX005_RaiseOnMissingTranslation()
    {
        await new VerifyCS.Test
        {
            TestState =
            {
                AdditionalFiles = {
                    ("/0/Resources.resx", ResxDocument("Name", "value")),
                    ("/0/Resources.de-DE.resx", ResxDocument("Name", "DE: value")),
                    ("/0/Resources.fr.resx", ResxDocument("Name", "FR: value")),
                    ("/0/Resources.es.resx", ResxEmptyDocument),
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult("DarpResX005", DiagnosticSeverity.Warning)
                        .WithSpan("/0/Resources.es.resx", 1, 1, 1, 1)
                        .WithArguments("Name", "Spanish", "es"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }
}
