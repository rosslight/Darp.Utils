namespace Darp.Utils.Messaging.Generator.Verify;

public sealed class MessageSourceTests
{
    [Fact]
    public async Task DefaultCase()
    {
        const string code = """
            using Darp.Utils.Messaging;

            namespace Test;

            [MessageSource]
            public sealed partial class TestMessageSource;
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }
}
