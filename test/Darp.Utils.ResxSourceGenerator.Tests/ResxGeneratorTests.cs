// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Darp.Utils.ResxSourceGenerator.Tests;

using Xunit;
using CSharpLanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VerifyCS = Verifiers.CSharpSourceGeneratorVerifier<ResxSourceGenerator.CSharpResxSourceGenerator>;

public class ResxGeneratorTests
{
    private const string ResxHeader = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
    <xsd:element name=""root"" msdata:IsDataSet=""true"">
      <xsd:complexType>
        <xsd:choice maxOccurs=""unbounded"">
          <xsd:element name=""metadata"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
              <xsd:attribute name=""type"" type=""xsd:string"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""assembly"">
            <xsd:complexType>
              <xsd:attribute name=""alias"" type=""xsd:string"" />
              <xsd:attribute name=""name"" type=""xsd:string"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""data"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
                <xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
              <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""resheader"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
    <value>2.0</value>
  </resheader>
  <resheader name=""reader"">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
";
    private const string ResxFooter = @"
</root>";

    [Theory]
    [InlineData(CSharpLanguageVersion.CSharp7, Skip = "Not supported (Missing nullable features)")]
    [InlineData(CSharpLanguageVersion.CSharp8)]
    [InlineData(CSharpLanguageVersion.CSharp9)]
//    [InlineData(CSharpLanguageVersion.CSharp10)]
//    [InlineData(CSharpLanguageVersion.CSharp11)]
//    [InlineData(CSharpLanguageVersion.CSharp12)]
    public async Task SingleString_DefaultCSharpAsync(CSharpLanguageVersion languageVersion)
    {
        var code = ResxHeader
            + @"  <data name=""Name"" xml:space=""preserve"">
    <value>value</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        await new VerifyCS.Test(identifier: languageVersion.ToString())
        {
            LanguageVersion = languageVersion,
            TestState =
            {
                Sources = { "" },
                AdditionalFiles = { ("/0/Resources.resx", code) },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Fact]
    public async Task TwoResourcesSameName_DefaultCSharpAsync()
    {
        var code = ResxHeader
            + @"  <data name=""Name"" xml:space=""preserve"">
    <value>value</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        await new VerifyCS.Test()
        {
            TestState =
            {
                Sources = { "" },
                AdditionalFiles =
                {
                    ("/0/First/Resources.resx", code),
                    ("/0/Second/Resources.resx", code),
                },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

build_property.RootNamespace = TestProject

[/0/First/Resources.resx]
build_metadata.AdditionalFiles.RelativeDir = First/

[/0/Second/Resources.resx]
build_metadata.AdditionalFiles.RelativeDir = Second/
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
        var code = ResxHeader
            + @"  <data name=""Name"" xml:space=""preserve"">
    <value>value</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        await new VerifyCS.Test(identifier: rootNamespace)
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", code) },
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
        var code = ResxHeader
            + @"  <data name=""Name"" xml:space=""preserve"">
    <value>value</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        await new VerifyCS.Test(identifier: relativeDir)
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", code) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

[/0/Resources.resx]
build_metadata.AdditionalFiles.RelativeDir = {relativeDir}
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
        var code = ResxHeader
            + @"  <data name=""Name"" xml:space=""preserve"">
    <value>value</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        await new VerifyCS.Test(identifier: className)
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", code) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

[/0/Resources.resx]
build_metadata.AdditionalFiles.ClassName = {className}
"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Theory]
    [CombinatorialData]
    public async Task SingleString_OmitGetResourceStringCSharpAsync(bool omitGetResourceString)
    {
        var code = ResxHeader
            + @"  <data name=""Name"" xml:space=""preserve"">
    <value>value</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        var customGetResourceString = @"#nullable enable

namespace TestProject
{
    internal static partial class Resources
    {
        internal static string? GetResourceString(string resourceKey, string? defaultValue = null) => throw null!;
    }
}
";

        await new VerifyCS.Test(identifier: omitGetResourceString.ToString())
        {
            TestState =
            {
                Sources = { omitGetResourceString ? customGetResourceString : "" },
                AdditionalFiles = { ("/0/Resources.resx", code) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

[/0/Resources.resx]
build_metadata.AdditionalFiles.OmitGetResourceString = {(omitGetResourceString ? "true" : "false")}
"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Theory]
    [CombinatorialData]
    public async Task SingleString_EmitFormatMethodsCSharpAsync(
        [CombinatorialValues("0", "x", "replacement")] string placeholder,
        bool emitFormatMethods)
    {
        var code = ResxHeader
            + $@"  <data name=""Name"" xml:space=""preserve"">
    <value>value {{{placeholder}}}</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        await new VerifyCS.Test(identifier: $"{placeholder}_{emitFormatMethods}")
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", code) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

[/0/Resources.resx]
build_metadata.AdditionalFiles.EmitFormatMethods = {(emitFormatMethods ? "true" : "false")}
"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }

    [Theory]
    [CombinatorialData]
    public async Task SingleString_PublicCSharpAsync(bool publicResource)
    {
        var code = ResxHeader
            + @"  <data name=""Name"" xml:space=""preserve"">
    <value>value</value>
    <comment>comment</comment>
  </data>"
            + ResxFooter;

        await new VerifyCS.Test(identifier: publicResource.ToString())
        {
            TestState =
            {
                AdditionalFiles = { ("/0/Resources.resx", code) },
                AnalyzerConfigFiles =
                {
                    ("/.globalconfig", $@"
is_global = true

[/0/Resources.resx]
build_metadata.AdditionalFiles.Public = {(publicResource ? "true" : "false")}
"),
                },
            },
        }.AddGeneratedSources().RunAsync();
    }
}
