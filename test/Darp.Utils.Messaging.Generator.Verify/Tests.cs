namespace Darp.Utils.Messaging.Generator.Verify;

public class Tests
{
    [Fact]
    public async Task DefaultCases()
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

                [MessageSink]
                private static void OnAnyStatic<T>(T message) where T : allows ref struct { }
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }

    [Fact]
    public async Task GenericClass()
    {
        const string code = """
            using System;
            using Darp.Utils.Messaging;

            namespace Test;

            public sealed partial class TestClass<T> where T : allows ref struct
            {
                [MessageSink]
                private void OnAny<T>(T message) { }
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }
}
