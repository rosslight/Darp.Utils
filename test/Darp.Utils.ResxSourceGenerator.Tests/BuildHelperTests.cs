namespace Darp.Utils.ResxSourceGenerator.Tests;

using System.Globalization;
using FluentAssertions;
using Xunit;

public class BuildHelperTests
{
    [Theory]
    [InlineData("Resources")]
    [InlineData("Localization.Resources")]
    [InlineData("Localization.Resources.enen-en-en-en")]
    [InlineData("Localization.Resources.Designer")]
    [InlineData("Asd.Localization.Resources.en")]
    [InlineData("Localization.0Resources.en")]
    [InlineData("Localization0.Resources.en")]
    [InlineData("Localization.Resources.e")]
    [InlineData("Localization.Resources0.de-DE")]
    [InlineData("Localization.Asd.Resources.en")]
    [InlineData("Localization.Asd.Resources.en.en")]
    [InlineData("Localization.Resources.d-DE")]
    [InlineData("Localization.Resources.enen", Skip = "Behaves differently between local PC and CI")]
    public void IsChildFile_ShouldClassifyParentFilesCorrectly(string fileToCheck)
    {
        string[] availableFiles = ["Localization.Resources", fileToCheck];
        var isChildFile = BuildHelper.IsChildFile(fileToCheck, availableFiles, out CultureInfo? cultureInfo);
        isChildFile.Should().BeFalse();
        cultureInfo.Should().BeNull();
    }

    [Theory]
    [InlineData("Localization.Resources.en", "en")]
    [InlineData("Localization.Resources.de-DE", "de-DE")]
    [InlineData("Localization.Resources.az-cyrl-az", "az-cyrl-az")]
    [InlineData("Localization.Resources.es-419", "es-419")]
    [InlineData("Localization.Resources.sr-latn-ba", "sr-latn-ba")]
    [InlineData("Localization.Resources.ia-001", "ia-001")]
    public void IsChildFile_ShouldClassifyChildFilesCorrectly(string fileToCheck, string expectedCultureString)
    {
        var expectedCulture = CultureInfo.GetCultureInfo(expectedCultureString);

        string[] availableFiles = ["Localization.Resources", fileToCheck];
        var isChildFile = BuildHelper.IsChildFile(fileToCheck, availableFiles, out CultureInfo? cultureInfo);
        isChildFile.Should().BeTrue();
        cultureInfo.Should().Be(expectedCulture);
    }

    [Theory]
    [InlineData("Asd", "Asd")]
    [InlineData("Asd.Asd", "Asd_Asd")]
    public void GetIdentifierFromResourceName_ShouldGetExpectedIdentifiers(string resourceName, string expectedPropertyIdentifier)
    {
        var propertyIdentifier = BuildHelper.GetIdentifierFromResourceName(resourceName);
        propertyIdentifier.Should().Be(expectedPropertyIdentifier);
    }
}
