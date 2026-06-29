namespace Darp.Utils.ResxSourceGenerator.Integration;

using System.Globalization;
using System.Reflection;
using Shouldly;
using Xunit;

public sealed class GeneratedResourcesTests
{
    [Fact]
    public void FormatWithFormatSpecifiers_ShouldFormatCorrectly()
    {
        var resources = new Resources { Culture = CultureInfo.InvariantCulture };
        var timestamp = new DateTime(2024, 1, 2, 13, 45, 0);

        resources.FormatNumericTime(timestamp).ShouldBe("Numeric 13:45");
        resources.FormatNamedTime(timestamp).ShouldBe("Named 13:45");
        resources.FormatNamedAligned(timestamp).ShouldBe("Aligned '      13'");
    }

    [Theory]
    [InlineData("en-US", "Amount 1,234.56")]
    [InlineData("de-DE", "Amount 1.234,56")]
    public void FormatMethods_ShouldUseConfiguredCultureForNumbers(string cultureName, string expectedValue)
    {
        var resources = new Resources { Culture = CultureInfo.GetCultureInfo(cultureName) };
        resources.FormatLocalizedNumber(1234.56).ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("{myName}", "myName", 0, "{0}")]
    [InlineData("{myName:T}", "myName", 0, "{0:T}")]
    [InlineData("{myName,10}", "myName", 0, "{0,10}")]
    [InlineData("{myName,10:T}", "myName", 0, "{0,10:T}")]
    [InlineData("{{myName:T}}", "myName", 0, "{{myName:T}}")]
    [InlineData("{myNameTotal:T}", "myName", 0, "{myNameTotal:T}")]
    [InlineData("{myName:T} {otherName:N2}", "otherName", 1, "{myName:T} {1:N2}")]
    public void GeneratedReplaceNamedFormatItem_ShouldPreserveCompositeFormatItemShape(
        string value,
        string formatterName,
        int index,
        string expectedValue
    )
    {
        InvokeGeneratedReplaceNamedFormatItem(value, formatterName, index).ShouldBe(expectedValue);
    }

    private static string InvokeGeneratedReplaceNamedFormatItem(string value, string formatterName, int index)
    {
        MethodInfo method =
            typeof(Resources).GetMethod("ReplaceNamedFormatItem", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(typeof(Resources).FullName, "ReplaceNamedFormatItem");

        return (string)method.Invoke(null, [value, formatterName, index])!;
    }
}
