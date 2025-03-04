namespace Darp.Utils.Messaging.Generator.Verify;

public sealed class CompiledTests { }

internal partial class Test2
{
    [MessageSink]
    private void OnInt(int a) { }

    [MessageSink]
    private static void OnInt2(int a) { }

    [MessageSink]
    private void OnTest3(ReadOnlySpan<byte> a) { }

    [MessageSink]
    private void OnTest4(ReadOnlySpan<byte> a) { }

    [MessageSink]
    private void OnTest5<T>(T a)
        where T : allows ref struct { }

    [MessageSink]
    private static void OnTest6<T>(T a)
        where T : allows ref struct { }
}
