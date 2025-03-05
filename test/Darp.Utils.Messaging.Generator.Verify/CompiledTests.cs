namespace Darp.Utils.Messaging.Generator.Verify;

using FluentAssertions;

#if False

public sealed class CompiledTests
{
    [Fact]
    public void ReceivedInt_ShouldBeReceived_WhenHandled()
    {
        const int intToPublish = 42;
        var subject = new MessageSubject();
        var sink = new TestIntHandler();

        subject.Subscribe(sink);
        subject.Publish(intToPublish);

        sink.ReceivedInt.Should().Be(intToPublish);
    }

    [Fact]
    public void ReceivedInt_ShouldNotBeReceived_WhenNotHandled()
    {
        const int intToPublish = 42;
        var subject = new MessageSubject();
        var sink = new TestStringHandler();

        subject.Subscribe(sink);
        subject.Publish(intToPublish);

        sink.ReceivedString.Should().BeNull();
    }

    [Fact]
    public void Dispose_ShouldWork_InsideCallback()
    {
        var source = new TestMessageSource();
        IDisposable? disp1 = null;

        disp1 = source.Subscribe<int>(_ => disp1?.Dispose());

        source.Publish(42);
    }

    [Fact]
    public void Subscribe_ShouldBeCalled_InOrderOfSubscription()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        source.Subscribe<int>(_ => list.Add(1));
        source.Subscribe<int>(_ => list.Add(2));
        source.Subscribe<int>(_ => list.Add(3));
        source.Subscribe<int>(_ => list.Add(4));

        source.Publish(42);

        list.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void Subscribe_ShouldBeCalled_WhenNotDisposed()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        IDisposable disp1 = source.Subscribe<int>(_ => list.Add(1));
        source.Subscribe<int>(_ => list.Add(2));
        IDisposable disp3 = source.Subscribe<int>(_ => list.Add(3));
        source.Subscribe<int>(_ => list.Add(4));

        disp1.Dispose();
        disp3.Dispose();
        source.Publish(42);

        list.Should().Equal(2, 4);
    }
}

internal partial class TestIntHandler
{
    public int ReceivedInt { get; private set; }

    [MessageSink]
    private void OnInt(int receivedInt) => ReceivedInt = receivedInt;
}

internal partial class TestStringHandler
{
    public string? ReceivedString { get; private set; }

    [MessageSink]
    private void OnInt(string? receivedString) => ReceivedString = receivedString;
}

[MessageSource]
internal partial class TestMessageSource
{
    public void Publish<T>(T message)
        where T : allows ref struct => PublishMessage(message);
}

internal partial class TestMessageSource : global::Darp.Utils.Messaging.IMessageSource
{
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.Utils.Messaging.Generator", "0.0.0.0")]
    [global::System.Obsolete("This field is not intended to be used in use code")]
    private readonly global::System.Collections.Generic.List<global::Darp.Utils.Messaging.IMessageSink> ___messageSinks =
        new global::System.Collections.Generic.List<global::Darp.Utils.Messaging.IMessageSink>();

    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.Utils.Messaging.Generator", "0.0.0.0")]
    [global::System.Obsolete("This field is not intended to be used in use code")]
#if NET9_0_OR_GREATER
    private readonly global::System.Threading.Lock ___lock = new Lock();
#else
    private readonly object ___lock = new object();
#endif

    /// <summary> Publish a new message </summary>
    /// <param name="message"> The message to be published </param>
    /// <typeparam name="T"> The type of the message to be published </typeparam>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.Utils.Messaging.Generator", "0.0.0.0")]
    protected void PublishMessage<T>(in T message)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        lock (___lock)
        {
            for (var index = ___messageSinks.Count - 1; index >= 0; index--)
            {
                global::Darp.Utils.Messaging.IMessageSink eventReceiver = ___messageSinks[index];
                if (eventReceiver is global::Darp.Utils.Messaging.IMessageSink<T> receiver)
                    receiver.Publish(message);
                else if (eventReceiver is global::Darp.Utils.Messaging.IAnyMessageSink anyReceiver)
                    anyReceiver.Publish(message);
            }
        }
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.Utils.Messaging.Generator", "0.0.0.0")]
    public global::System.IDisposable Subscribe(global::Darp.Utils.Messaging.IMessageSink sink)
    {
        lock (___lock)
        {
            ___messageSinks.Insert(0, sink);
            return global::Darp.Utils.Messaging.FuncDisposable.Create(
                (_lock: ___lock, _eventReceiverProxies: ___messageSinks, sink),
                state =>
                {
                    lock (state._lock)
                        state._eventReceiverProxies.Remove(state.sink);
                }
            );
        }
    }
}
#endif
