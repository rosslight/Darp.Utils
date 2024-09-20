namespace Darp.Utils.Dialog.FluentAvalonia.Tests;

using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.DialogData;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

public sealed class SubstituteDialogService : IDialogService
{
    public IContentDialogBuilder<TContent> CreateContentDialog<TContent>(string title, TContent content)
    {
        IContentDialogBuilder<TContent> subBuilder = Substitute.For<IContentDialogBuilder<TContent>>();
        subBuilder.Title.Returns(title);
        subBuilder.Content.Returns(content);
        return subBuilder;
    }

    public void Dispose()
    {
    }
}

public class AvaloniaDialogServiceTests
{
    [Fact]
    public void UsingDI_AddDialogService_ShouldNotThrow()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddSingleton<IDialogService, AvaloniaDialogService>()
            .BuildServiceProvider();

        // Act
        Action act = () => provider.GetRequiredService<IDialogService>();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Initialize_ShouldSetProperties()
    {
        // Arrange
        const string title = "Title";
        const int content = 42;
        var service = new AvaloniaDialogService();

        // Act
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, title, content));

        // Assert
        builder.Title.Should().Be(title);
        builder.Content.Should().Be(content);
        builder.Dialog.DataTemplates.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(ContentDialogButton.Close)]
    [InlineData(ContentDialogButton.Primary)]
    [InlineData(ContentDialogButton.Secondary)]
    public void SetDefaultButton_ShouldSetCorrectButton(ContentDialogButton button)
    {
        // Arrange
        var service = new AvaloniaDialogService();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42);

        // Act
        IContentDialogBuilder<int> returnedBuilder = builder.SetDefaultButton(button);

        // Assert
        builder.Should().BeEquivalentTo(returnedBuilder);
        builder.Dialog.DefaultButton.Should().Be((global::FluentAvalonia.UI.Controls.ContentDialogButton)button);
    }

    [Fact]
    public void SetCloseButton_ShouldBeSetUpCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42);

        // Act
        IContentDialogBuilder<int> returnedBuilder = builder.SetCloseButton(buttonText);

        // Assert
        builder.Should().BeEquivalentTo(returnedBuilder);
        builder.Dialog.CloseButtonText.Should().Be(buttonText);
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task SetCloseButton_OnClick_ShouldFireCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        const int content = 42;
        var service = new AvaloniaDialogService();
        var window = new Window();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", content, window);

        // Act
        window.Show();
        int? contentReceivedOnClick = null;
        IContentDialogBuilder<int> returnedBuilder = builder
            .SetCloseButton(buttonText, c =>
            {
                contentReceivedOnClick = c;
                return true;
            });
        builder.Dialog.Opened += (sender, _) =>
        {
            Button button = sender.GetFirstDescendant<Button>("CloseButton");
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            window.MouseClick(button.GetRelativeBounds(window).Center, MouseButton.Left);
        };
        ContentDialogResult<int> result = await builder.ShowAsync();

        // Assert
        builder.Should().BeEquivalentTo(returnedBuilder);
        contentReceivedOnClick.Should().Be(content);
        result.Result.Should().Be(ContentDialogResult.None);
        result.Content.Should().Be(content);
        window.Close();
    }

    [Fact]
    public void SetPrimaryButton_ShouldBeSetUpCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42);

        // Act
        IContentDialogBuilder<int> returnedBuilder = builder.SetPrimaryButton(buttonText);

        // Assert
        builder.Should().BeEquivalentTo(returnedBuilder);
        builder.Dialog.PrimaryButtonText.Should().Be(buttonText);
    }

    [Fact]
    public void SetPrimaryButton_IsEnabled_ShouldBeBoundCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42));
        var subject = new BehaviorSubject<bool>(false);

        // Act and assert
        IContentDialogBuilder<int> returnedBuilder = builder.SetPrimaryButton(buttonText, isEnabled: subject);

        builder.Should().BeEquivalentTo(returnedBuilder);
        builder.Dialog.IsPrimaryButtonEnabled.Should().BeFalse();

        subject.OnNext(true);
        builder.Dialog.IsPrimaryButtonEnabled.Should().BeTrue();

        subject.OnNext(false);
        builder.Dialog.IsPrimaryButtonEnabled.Should().BeFalse();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task SetPrimaryButton_OnClick_ShouldFireCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        const int content = 42;
        var service = new AvaloniaDialogService();
        var window = new Window();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", content, window);

        // Act
        window.Show();
        Dispatcher.UIThread.RunJobs();
        int? contentReceivedOnClick = null;
        IContentDialogBuilder<int> returnedBuilder = builder
            .SetPrimaryButton(buttonText, onClick: (c, _) =>
            {
                contentReceivedOnClick = c;
                return Task.FromResult(true);
            });
        builder.Dialog.Opened += (sender, _) =>
        {
            Button button = sender.GetFirstDescendant<Button>("PrimaryButton");
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            window.MouseClick(button.GetRelativeBounds(window).Center, MouseButton.Left);
        };
        ContentDialogResult<int> result = await builder.ShowAsync();

        // Assert
        builder.Should().BeEquivalentTo(returnedBuilder);
        contentReceivedOnClick.Should().Be(content);
        result.Result.Should().Be(ContentDialogResult.Primary);
        result.Content.Should().Be(content);
        window.Close();
    }

    [Fact]
    public void SetSecondaryButton_ShouldBeSetUpCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42);

        // Act
        IContentDialogBuilder<int> returnedBuilder = builder.SetSecondaryButton(buttonText);

        // Assert
        builder.Should().BeEquivalentTo(returnedBuilder);
        builder.Dialog.SecondaryButtonText.Should().Be(buttonText);
    }

    [Fact]
    public void SetSecondaryButton_IsEnabled_ShouldBeBoundCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42));
        var subject = new BehaviorSubject<bool>(false);

        // Act and assert
        IContentDialogBuilder<int> returnedBuilder = builder.SetSecondaryButton(buttonText, isEnabled: subject);

        builder.Should().BeEquivalentTo(returnedBuilder);
        builder.Dialog.IsSecondaryButtonEnabled.Should().BeFalse();

        subject.OnNext(true);
        builder.Dialog.IsSecondaryButtonEnabled.Should().BeTrue();

        subject.OnNext(false);
        builder.Dialog.IsSecondaryButtonEnabled.Should().BeFalse();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task SetSecondaryButton_OnClick_ShouldFireCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        const int content = 42;
        var service = new AvaloniaDialogService();
        var window = new Window();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", content, window);

        // Act
        window.Show();
        Dispatcher.UIThread.RunJobs();
        int? contentReceivedOnClick = null;
        IContentDialogBuilder<int> returnedBuilder = builder
            .SetSecondaryButton(buttonText, onClick: (c, _) =>
            {
                contentReceivedOnClick = c;
                return Task.FromResult(true);
            });
        builder.Dialog.Opened += (sender, _) =>
        {
            Button button = sender.GetFirstDescendant<Button>("SecondaryButton");
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            window.MouseClick(button.GetRelativeBounds(window).Center, MouseButton.Left);
        };
        ContentDialogResult<int> result = await builder.ShowAsync();

        // Assert
        builder.Should().BeEquivalentTo(returnedBuilder);
        contentReceivedOnClick.Should().Be(content);
        result.Result.Should().Be(ContentDialogResult.Secondary);
        result.Content.Should().Be(content);
        window.Close();
    }

    [AvaloniaTheory(Timeout = 5000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MessageBoxModel_ShowAsync_ShouldHaveValidTemplates(bool isSelectable)
    {
        // Arrange
        const string messageText = "message";
        var content = new MessageBoxModel
        {
            Message = messageText,
            IsSelectable = isSelectable,
        };
        var service = new AvaloniaDialogService();
        var window = new Window();
        var builder = new FluentAvaloniaContentDialogBuilder<MessageBoxModel>(service, "Title", content, window);

        // Act
        window.Show();
        Dispatcher.UIThread.RunJobs();
        bool? isSelectableTextBlock = null;
        builder.Dialog.Opened += (sender, _) =>
        {
            TextBlock? textBlock = sender.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(x => x.IsVisible && x.Text == messageText);
            if (textBlock is not null)
            {
                isSelectableTextBlock = false;
            }
            SelectableTextBlock? selectableTextBlock = sender.GetVisualDescendants()
                .OfType<SelectableTextBlock>()
                .FirstOrDefault(x => x.IsVisible && x.Text == messageText);
            if (selectableTextBlock is not null)
            {
                isSelectableTextBlock = true;
            }
            builder.Dialog.Hide();
        };
        ContentDialogResult<MessageBoxModel> result = await builder.ShowAsync();

        // Assert
        isSelectableTextBlock.Should().Be(isSelectable);
        result.Result.Should().Be(ContentDialogResult.None);
        window.Close();
    }
}

file static class Extensions
{
    public static T GetFirstDescendant<T>(this Visual visual, string name) where T : StyledElement =>
        visual.GetVisualDescendants().GetFirstDescendant<T>(name);

    public static T GetFirstDescendant<T>(this IEnumerable<Visual> source, string name) where T : StyledElement =>
        source.OfType<T>().First(x => x.Name == name);

    /// <summary>
    /// Get the bounds of the first visual relative to the second visual.
    /// See https://github.com/amwx/FluentAvalonia/blob/2264dfe44bce8b0f532f58909a47cd5edf9acdf7/tests/FluentAvaloniaTests/ControlTests/ContentDialogTests.cs#L167
    /// </summary>
    /// <param name="visual"> The visual to get the bounds from</param>
    /// <param name="second">The visual to calculate the bounds to relatively</param>
    /// <returns>The bounds of the first visual</returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if transform between visuals could not be calculated </exception>
    public static Rect GetRelativeBounds(this Visual visual, Visual second)
    {
        Matrix? transform = visual.TransformToVisual(second) ?? throw new ArgumentOutOfRangeException(null, "Could not calculate transformation from one visual to the other");
        return new Rect(visual.Bounds.Size).TransformToAABB(transform.Value);
    }

    /// <summary> Simulates a mouse click (down and up) on the headless window/toplevel. </summary>
    public static void MouseClick(
        this TopLevel topLevel,
        Point point,
        MouseButton button,
        RawInputModifiers modifiers = RawInputModifiers.None)
    {
        topLevel.MouseDown(point, button, modifiers);
        topLevel.MouseUp(point, button, modifiers);
    }
}
