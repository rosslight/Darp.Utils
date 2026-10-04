namespace Darp.Utils.ResxSourceGenerator.Tests;

using Shouldly;
using Xunit;

public sealed class ResourceFormatHelperTests
{
    [Theory]
    [InlineData("{0:T}", false, "0")]
    [InlineData("{myName:T}", true, "myName")]
    [InlineData("{0,10}", false, "0")]
    [InlineData("{myName,10}", true, "myName")]
    [InlineData("{0,10:T}", false, "0")]
    [InlineData("{myName,10:T}", true, "myName")]
    [InlineData("{0,-10}", false, "0")]
    [InlineData("{myName,-10:T}", true, "myName")]
    [InlineData("{0,10 }", false, "0")]
    [InlineData("{0 :N2}", false, "0")]
    [InlineData("{myName , -10 :N2}", true, "myName")]
    [InlineData("{año}", true, "año")]
    [InlineData("{name_ä:T}", true, "name_ä")]
    [InlineData("{class}", true, "class")]
    [InlineData("{{{0}}}", false, "0")]
    [InlineData("{0}}}", false, "0")]
    [InlineData("{{{myName:T}}}", true, "myName")]
    [InlineData("{myName}}}", true, "myName")]
    public void GetArguments_ShouldFindArgumentsInCompositeFormatItems(
        string value,
        bool expectedUsingNamedArgs,
        string expectedArgument
    )
    {
        IReadOnlyList<string> arguments = ResourceFormatHelper.GetArguments(value, out FormatArgumentStyle style);

        style.ShouldBe(expectedUsingNamedArgs ? FormatArgumentStyle.Named : FormatArgumentStyle.Numbered);
        arguments.ShouldBe([expectedArgument]);
    }

    [Theory]
    [InlineData("{0} {1} {2}", "0", "1", "2")]
    [InlineData("{2} {0} {1}", "0", "1", "2")]
    [InlineData("{1:T}", "0", "1")]
    [InlineData("{myName} {otherName}", "myName", "otherName")]
    [InlineData("{myName} {myNameTotal} {myName}", "myName", "myNameTotal")]
    [InlineData("{2} {0} {2}", "0", "1", "2")]
    public void GetArguments_ShouldReturnDistinctArgumentsInStableOrder(string value, params string[] expectedArguments)
    {
        IReadOnlyList<string> arguments = ResourceFormatHelper.GetArguments(value, out FormatArgumentStyle _);

        arguments.ShouldBe(expectedArguments);
    }

    [Theory]
    [InlineData("{{0}}")]
    [InlineData("{{myName:T}}")]
    public void GetArguments_ShouldIgnoreEscapedBraceLiterals(string value)
    {
        IReadOnlyList<string> arguments = ResourceFormatHelper.GetArguments(value, out FormatArgumentStyle style);

        style.ShouldBe(FormatArgumentStyle.None);
        arguments.ShouldBeEmpty();
    }

    [Fact]
    public void GetArguments_ShouldAllowHighestSupportedNumericIndex()
    {
        IReadOnlyList<string> arguments = ResourceFormatHelper.GetArguments("{255}", out FormatArgumentStyle style);

        style.ShouldBe(FormatArgumentStyle.Numbered);
        arguments.Count.ShouldBe(256);
        arguments[0].ShouldBe("0");
        arguments[255].ShouldBe("255");
    }

    [Fact]
    public void GetArguments_ShouldRejectMixedNamedAndNumberedArguments()
    {
        IReadOnlyList<string> arguments = ResourceFormatHelper.GetArguments(
            "{0} {myName}",
            out FormatArgumentStyle style
        );

        style.ShouldBe(FormatArgumentStyle.Mixed);
        arguments.ShouldBeEmpty();
    }
}
