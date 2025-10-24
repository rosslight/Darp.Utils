namespace Darp.Utils.Messaging.Generator.Verify;

using System.Diagnostics;
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

    [Fact]
    public void Subscribe_ShouldWork_WhenCalledDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        IDisposable? newSubscription = null;

        // First subscriber that will add a new subscription during callback
        source.Subscribe<int>(_ =>
        {
            list.Add(1);
            newSubscription.Should().BeNull();
            newSubscription = source.Subscribe<int>(_ => throw new UnreachableException());
        });

        // Second subscriber to ensure the new subscription is added during iteration
        source.Subscribe<int>(_ => list.Add(2));

        source.Publish(42);

        // The new subscription should NOT be called in the same publish cycle
        // because it was added after the publish cycle started
        list.Should().Equal(1, 2);
    }

    [Fact]
    public void Subscribe_ShouldWork_WhenMultipleSubscriptionsAddedDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();

        source.Subscribe<int>(_ =>
        {
            list.Add(1);
            source.Subscribe<int>(_ => throw new UnreachableException());
            source.Subscribe<int>(_ => throw new UnreachableException());
        });
        source.Subscribe<int>(_ => list.Add(2));

        source.Publish(42);

        // New subscriptions should NOT be called in the same publish cycle
        // because they were added after the publish cycle started
        list.Should().Equal(1, 2);
    }

    [Fact]
    public void Unsubscribe_ShouldWork_WhenCalledDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        IDisposable? subscriptionToRemove = null;

        // First subscriber that will remove itself during callback
        subscriptionToRemove = source.Subscribe<int>(_ =>
        {
            list.Add(1);
            subscriptionToRemove!.Dispose();
        });

        // Second subscriber to ensure the first one is properly removed
        source.Subscribe<int>(_ => list.Add(2));

        source.Publish(42);
        source.Publish(42);

        // Only the second subscriber should be called
        list.Should().Equal(1, 2, 2);
    }

    [Fact]
    public void Unsubscribe_ShouldWork_WhenMultipleUnsubscriptionsDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        IDisposable? disp2 = null;

        // First subscriber that should remain
        source.Subscribe<int>(_ => list.Add(1));

        // Second subscriber that will remove another during callback
        IDisposable disp1 = source.Subscribe<int>(_ =>
        {
            list.Add(2);
            disp2!.Dispose();
        });

        // Third subscriber that will be removed by the first
        disp2 = source.Subscribe<int>(_ =>
        {
            list.Add(3);
            disp1.Dispose();
        });

        source.Publish(42);
        source.Publish(42);

        list.Should().Equal(1, 2, 3, 1);
    }

    [Fact]
    public void SubscribeAndUnsubscribe_ShouldWork_WhenCalledDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        IDisposable? subscriptionToRemove = null;
        IDisposable? newSubscription = null;

        // First subscriber that will remove itself and add a new one during callback
        subscriptionToRemove = source.Subscribe<int>(_ =>
        {
            list.Add(1);
            subscriptionToRemove!.Dispose();
            newSubscription = source.Subscribe<int>(_ => list.Add(3));
        });

        // Second subscriber
        source.Subscribe<int>(_ => list.Add(2));

        source.Publish(42);
        source.Publish(42);

        // The new subscription should NOT be called in the same publish cycle
        // because it was added after the publish cycle started
        list.Should().Equal(1, 2, 2, 3);
    }

    [Fact]
    public void Subscribe_ShouldNotAffect_CurrentPublishCycle_WhenAddedDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        var publishCount = 0;

        source.Subscribe<int>(_ =>
        {
            list.Add(1);

            // Add a new subscription during the first publish
            if (publishCount != 0)
                return;
            publishCount++;
            source.Subscribe<int>(_ => list.Add(2));
        });

        source.Publish(42);
        source.Publish(43);

        // First publish: only original subscriber (1)
        // Second publish: both original subscriber (2) and new subscriber (99)
        list.Should().Equal(1, 1, 2);
    }

    [Fact]
    public void Unsubscribe_ShouldNotAffect_CurrentPublishCycle_WhenRemovedDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        IDisposable? subscriptionToRemove = null;
        var publishCount = 0;

        subscriptionToRemove = source.Subscribe<int>(_ =>
        {
            list.Add(1);

            // Remove subscription during the first publish
            if (publishCount != 0)
                return;
            publishCount++;
            subscriptionToRemove!.Dispose();
        });

        source.Subscribe<int>(_ => list.Add(2));

        source.Publish(42);
        source.Publish(43);

        // First publish: both subscribers (1, 100)
        // Second publish: only remaining subscriber (100)
        list.Should().Equal(1, 2, 2);
    }

    [Fact]
    public void MultipleUnsubscriptions_ShouldBeRemoved_WhenRemovedDuringCallback()
    {
        List<int> list = [];
        var source = new TestMessageSource();
        IDisposable? disp1 = null;
        IDisposable? disp2 = null;
        IDisposable? disp3 = null;

        // This test demonstrates a limitation of the current implementation
        // When multiple subscriptions are removed during callback execution,
        // it can cause an index out of range exception due to list modification during iteration

        disp1 = source.Subscribe<int>(_ => list.Add(1));
        disp2 = source.Subscribe<int>(_ =>
        {
            list.Add(2);
            disp1!.Dispose();
            disp2!.Dispose();
            disp3!.Dispose();
        });
        disp3 = source.Subscribe<int>(_ => list.Add(3));

        source.Publish(42);
        source.Publish(42);

        list.Should().Equal(1, 2, 3);
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
