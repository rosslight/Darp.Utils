namespace Darp.Utils.Messaging.Generator.Verify;

public class Tests
{
    [Fact]
    public async Task Asd()
    {
        const string code = """
            using System;
            using Darp.Utils.Messaging;

            namespace Test;

            public sealed partial class TestClass
            {
                [MessageSink]
                private void OnInt(int message) { }

                [MessageSink]
                private static void OnIntStatic(int message) { }

                [MessageSink]
                private void OnSpan(ReadOnlySpan<byte> message) { }

                [MessageSink]
                private void OnAny<T>(T message) where T : allows ref struct { }
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }
}
