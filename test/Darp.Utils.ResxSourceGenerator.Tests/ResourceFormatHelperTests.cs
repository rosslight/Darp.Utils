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
    public void GetArguments_ShouldFindArgumentsInCompositeFormatItems(
        string value,
        bool expectedUsingNamedArgs,
        string expectedArgument
    )
    {
        IReadOnlyList<string> arguments = ResourceFormatHelper.GetArguments(value, out var usingNamedArgs);

        usingNamedArgs.ShouldBe(expectedUsingNamedArgs);
        arguments.ShouldBe([expectedArgument]);
    }

    [Theory]
    [InlineData("{0} {1} {2}", "0", "1", "2")]
    [InlineData("{2} {0} {1}", "0", "1", "2")]
    [InlineData("{myName} {otherName}", "myName", "otherName")]
    public void GetArguments_ShouldReturnDistinctArgumentsInStableOrder(string value, params string[] expectedArguments)
    {
        IReadOnlyList<string> arguments = ResourceFormatHelper.GetArguments(value, out _);

        arguments.ShouldBe(expectedArguments);
    }
}
