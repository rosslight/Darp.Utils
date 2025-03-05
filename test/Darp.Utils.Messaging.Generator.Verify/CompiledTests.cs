namespace Darp.Utils.Messaging.Generator.Verify;

using FluentAssertions;

#if True

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
    [MessageSink]
    public void Publish<T>(T message)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        => PublishMessage(message);
}
#endif
