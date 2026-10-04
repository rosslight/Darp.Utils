namespace Darp.Utils.ResxSourceGenerator.Integration;

using System.Globalization;
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
        resources.FormatNumericSecondTime("ignored", timestamp).ShouldBe("Numeric second 13:45");
        resources.FormatNamedTime(timestamp).ShouldBe("Named 13:45");
        resources.FormatNamedAligned(timestamp).ShouldBe("Aligned '      13'");
        resources.FormatNamedBracedTime(timestamp).ShouldBe("Braced {13:45}");
        resources.FormatNamedTrailingBrace(timestamp).ShouldBe("Trailing 13:45}");
    }

    [Theory]
    [InlineData("en-US", "Amount 1,234.56")]
    [InlineData("de-DE", "Amount 1.234,56")]
    public void FormatMethods_ShouldUseConfiguredCultureForNumbers(string cultureName, string expectedValue)
    {
        var resources = new Resources { Culture = CultureInfo.GetCultureInfo(cultureName) };
        resources.FormatLocalizedNumber(1234.56).ShouldBe(expectedValue);
    }

    [Fact]
    public void FormatMethods_ShouldPreserveNamesEscapesAndAlignment()
    {
        var resources = new Resources { Culture = CultureInfo.InvariantCulture };

        resources.FormatNamedRepeated("Ada", "Lovelace").ShouldBe("Ada / Lovelace / Ada");
        resources.FormatNamedEscaped("Ada").ShouldBe("{name} / {Ada} / Ada}");
        resources.FormatNamedWhitespace(42).ShouldBe("'42.00     '");
        resources.FormatNumericWhitespace(42).ShouldBe("'     42.00'");
        resources.FormatUnicodeAndKeyword(año: 2026, @class: "A").ShouldBe("2026 / A");
        resources
            .FormatMemberNames(Culture: 42, MemberNames: "Ada", GetResourceString: "X")
            .ShouldBe("42.00 / Ada / X");
        resources.FormatNumericReordered("first", "unused", "third").ShouldBe("third / first / third");
    }

    [Fact]
    public void FormatMethods_ShouldUseDefaultParameterOrderAcrossTranslations()
    {
        var resources = new Resources { Culture = CultureInfo.InvariantCulture };

        resources.FormatTranslated("Ada", "Lovelace", 1234.56).ShouldBe("Ada Lovelace: 1,234.56");
        resources.Culture = CultureInfo.GetCultureInfo("de-DE");
        resources.FormatTranslated("Ada", "Lovelace", 1234.56).ShouldBe("Lovelace, Ada: 1.234,56 (Ada)");
        resources.Translated.ShouldBe("{lastName}, {firstName}: {amount:N2} ({firstName})");
        Resources.Keys.Translated.ShouldBe("Translated");
    }

    [Fact]
    public void RawProperties_ShouldPreserveTemplatesAndAvoidSecondResourceLookup()
    {
        var resources = new Resources { Culture = CultureInfo.InvariantCulture };

        resources.NamedEscaped.ShouldBe("{{name}} / {{{name}}} / {name}}}");
        resources.EscapedOnly.ShouldBe("{{name}}");
        resources.FormatLookupCollision("Ada").ShouldBe("Ada");
    }

    [Fact]
    public void FormatMethods_ShouldDelegateValueFormattingToDotNet()
    {
        var resources = new Resources { Culture = CultureInfo.InvariantCulture };

        resources.FormatNamedRepeated(null, "Lovelace").ShouldBe(" / Lovelace / ");
        Should.Throw<FormatException>(() => resources.FormatInvalidSpecifier(42));
    }
}
