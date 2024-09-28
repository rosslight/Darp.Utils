// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Darp.Utils.ResxSourceGenerator.Tests;

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
    public async Task SingleString_DefaultCSharpAsync(CSharpLanguageVersion languageVersion)
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
    public async Task TwoResourcesSameName_DefaultCSharpAsync()
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
    public async Task SingleString_RootNamespaceCSharpAsync(string rootNamespace)
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
    public async Task SingleString_RelativeDirCSharpAsync(string relativeDir)
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
    public async Task SingleString_ClassNameCSharpAsync(string className)
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
    [InlineData("0")]
    [InlineData("replacement")]
    [InlineData("x")]
    public async Task SingleString_EmitFormatMethodsCSharpAsync(string placeholder)
    {
        var code = ResxValueWithPlaceholderDocument(placeholder);

        await new VerifyCS.Test(identifier: placeholder)
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
    public async Task SingleString_PublicCSharpAsync()
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
}
