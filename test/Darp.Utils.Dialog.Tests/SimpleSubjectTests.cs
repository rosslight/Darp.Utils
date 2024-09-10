namespace Darp.Utils.Dialog.Tests;

using System.Reactive.Subjects;
using Darp.Utils.Dialog.Helper;
using FluentAssertions;
using FluentAssertions.Reactive;
using Microsoft.Reactive.Testing;

public class SimpleSubjectTests
{
    [Fact]
    public void OnNext_ShouldNotifyAllSubscribers()
    {
        // Arrange
        var scheduler = new TestScheduler();
        var x = new SimpleSubject<string>();
        using FluentTestObserver<string> observedSequence = x.Observe(scheduler);

        // Act
        x.OnNext("TestTestTest");
        x.OnNext("TestTestTest");
        x.OnNext("TestTestTest");
        scheduler.AdvanceBy(1);

        // Assert
        observedSequence.RecordedNotifications.Should().HaveCount(3);
    }

    [Fact]
    public void OnError_ShouldNotifyAllSubscribers()
    {
        // Arrange
        var error = new ArgumentException("Some argument was bad");
        var x = new SimpleSubject<string>();
        using FluentTestObserver<string> observedSequence = x.Observe();

        // Act
        x.OnError(error);

        // Assert
        observedSequence.Error.Should().Be(error);
    }

    [Fact]
    public void OnCompleted_ShouldNotifyAllSubscribers()
    {
        // Arrange
        var x = new SimpleSubject<string>();
        using FluentTestObserver<string> observedSequence = x.Observe();

        // Act
        x.OnCompleted();

        // Assert
        observedSequence.Should().Complete();
    }
}
