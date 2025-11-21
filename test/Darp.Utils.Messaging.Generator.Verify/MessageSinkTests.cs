namespace Darp.Utils.Messaging.Generator.Verify;

public sealed class MessageSinkTests
{
#if NET9_0_OR_GREATER
    [Fact]
#endif
    public async Task DefaultCases_Net9()
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
                private void OnAny<T>(T message)
                    where T : allows ref struct
                { }

                [MessageSink]
                private static void OnAnyStatic<T>(T message)
                    where T : allows ref struct
                { }
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }

#if !NET9_0_OR_GREATER
    [Fact]
#endif
    public async Task DefaultCases_BelowNet9()
    {
        const string code = """
            using Darp.Utils.Messaging;

            namespace Test;

            public sealed partial class TestClass
            {
                [MessageSink]
                private void OnInt(int message) { }

                [MessageSink]
                private static void OnIntStatic(int message) { }

                [MessageSink]
                private void OnAny<T>(T message) { }

                [MessageSink]
                private static void OnAnyStatic<T>(T message) { }
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }

    [Fact]
    public async Task GenericClass()
    {
        const string code = """
            using Darp.Utils.Messaging;

            namespace Test;

            public sealed partial class TestClass<T1, T2> where T1 : class
            {
                [MessageSink]
                private void OnT(T1 message) { }
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }

    [Fact]
    public async Task TestSubject()
    {
        const string code = """
            using Darp.Utils.Messaging;

            namespace Test;

            [MessageSource]
            public sealed partial class TestSubject
            {
                [MessageSink]
                public void Publish<T>(T message)
            #if NET9_0_OR_GREATER
                    where T : allows ref struct
            #endif
                { }
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }

    [Fact]
    public async Task AsyncMethod()
    {
        const string code = """
            using System.Threading.Tasks;
            using Darp.Utils.Messaging;

            namespace Test;

            public sealed partial class TestSubject
            {
                [MessageSink]
                public ValueTask OnIntAsync(int message) => ValueTask.CompletedTask;
                [MessageSink]
                public void OnInt(int message) { }
                [MessageSink]
                public static ValueTask OnIntStaticAsync(int message) => ValueTask.CompletedTask;
            }
            """;
        await VerifyHelper.VerifyMessagingGenerator(code);
    }
}
