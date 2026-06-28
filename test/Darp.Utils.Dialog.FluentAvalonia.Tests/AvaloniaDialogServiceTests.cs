namespace Darp.Utils.Dialog.FluentAvalonia.Tests;

using System.Collections;
using System.Reactive.Subjects;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.DialogData;
using global::FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

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
        act.ShouldNotThrow();
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
            new FluentAvaloniaContentDialogBuilder<int>(service, title, content)
        );

        // Assert
        builder.Title.ShouldBe(title);
        builder.Content.ShouldBe(content);
        builder.Dialog.DataTemplates.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData(ContentDialogButton.Close)]
    [InlineData(ContentDialogButton.Primary)]
    [InlineData(ContentDialogButton.Secondary)]
    public void SetDefaultButton_ShouldSetCorrectButton(ContentDialogButton button)
    {
        // Arrange
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42)
        );

        // Act
        IContentDialogBuilder<int> returnedBuilder = builder.SetDefaultButton(button);

        // Assert
        builder.ShouldBeEquivalentTo(returnedBuilder);
        builder.Dialog.DefaultButton.ShouldBe((global::FluentAvalonia.UI.Controls.FAContentDialogButton)button);
    }

    [Fact]
    public void SetCloseButton_ShouldBeSetUpCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42)
        );

        // Act
        IContentDialogBuilder<int> returnedBuilder = builder.SetCloseButton(buttonText);

        // Assert
        builder.ShouldBeEquivalentTo(returnedBuilder);
        builder.Dialog.CloseButtonText.ShouldBe(buttonText);
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task SetCloseButton_OnClick_ShouldFireCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        const int content = 42;
        var service = new AvaloniaDialogService();
        var window = new Window();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", content, window)
        );

        // Act
        window.Show();
        int? contentReceivedOnClick = null;
        IContentDialogBuilder<int> returnedBuilder = builder.SetCloseButton(
            buttonText,
            c =>
            {
                contentReceivedOnClick = c;
                return true;
            }
        );
        builder.Dialog.Opened += (sender, _) =>
        {
            Button button = sender.GetFirstDescendant<Button>("CloseButton");
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            window.MouseClick(button.GetRelativeBounds(window).Center, MouseButton.Left);
        };
        ContentDialogResult<int> result = await builder.ShowAsync();

        // Assert
        builder.ShouldBeEquivalentTo(returnedBuilder);
        contentReceivedOnClick.ShouldBe(content);
        result.Result.ShouldBe(ContentDialogResult.None);
        result.Content.ShouldBe(content);
        window.Close();
    }

    [Fact]
    public void SetPrimaryButton_ShouldBeSetUpCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42)
        );

        // Act
        IContentDialogBuilder<int> returnedBuilder = builder.SetPrimaryButton(buttonText);

        // Assert
        builder.ShouldBeEquivalentTo(returnedBuilder);
        builder.Dialog.PrimaryButtonText.ShouldBe(buttonText);
    }

    [Fact]
    public void SetPrimaryButton_IsEnabled_ShouldBeBoundCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42)
        );
        var subject = new BehaviorSubject<bool>(false);

        // Act and assert
        IContentDialogBuilder<int> returnedBuilder = builder.SetPrimaryButton(buttonText, isEnabled: subject);

        builder.ShouldBeEquivalentTo(returnedBuilder);
        builder.Dialog.IsPrimaryButtonEnabled.ShouldBeFalse();

        subject.OnNext(true);
        builder.Dialog.IsPrimaryButtonEnabled.ShouldBeTrue();

        subject.OnNext(false);
        builder.Dialog.IsPrimaryButtonEnabled.ShouldBeFalse();
    }

    [Fact]
    public void SetPrimaryButton_IsEnabled_ShouldNotRegisterDialogDisposablesOnService()
    {
        // Arrange
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42)
        );
        var subject = new BehaviorSubject<bool>(true);

        // Act
        builder.SetPrimaryButton("Ok", isEnabled: subject);

        // Assert
        service.GetRegisteredDisposableCount().ShouldBe(0);
    }

    [Fact]
    public void SetPrimaryButton_IsEnabled_ShouldCancelPrimaryClick_WhenDisabled()
    {
        // Arrange
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42)
        );
        var subject = new BehaviorSubject<bool>(false);
        var wasClicked = false;

        // Act
        builder.SetPrimaryButton(
            "Ok",
            isEnabled: subject,
            onClick: (_, _) =>
            {
                wasClicked = true;
                return Task.FromResult(true);
            }
        );
        // Invoke the dialog hook directly: keyboard routing is not reliable in headless tests,
        // and this is the path Enter/default-button activation ultimately reaches.
        global::FluentAvalonia.UI.Controls.FAContentDialogButtonClickEventArgs args =
            Extensions.InvokePrimaryButtonClick(builder.Dialog);

        // Assert
        wasClicked.ShouldBeFalse();
        args.Cancel.ShouldBeTrue();
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
        IContentDialogBuilder<int> returnedBuilder = builder.SetPrimaryButton(
            buttonText,
            onClick: (c, _) =>
            {
                contentReceivedOnClick = c;
                return Task.FromResult(true);
            }
        );
        builder.Dialog.Opened += (sender, _) =>
        {
            Button button = sender.GetFirstDescendant<Button>("PrimaryButton");
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            window.MouseClick(button.GetRelativeBounds(window).Center, MouseButton.Left);
        };
        ContentDialogResult<int> result = await builder.ShowAsync();

        // Assert
        builder.ShouldBeEquivalentTo(returnedBuilder);
        contentReceivedOnClick.ShouldBe(content);
        result.Result.ShouldBe(ContentDialogResult.Primary);
        result.Content.ShouldBe(content);
        window.Close();
    }

    [AvaloniaFact(Timeout = 5000)]
    public async Task ShowAsync_DisposeAwaitable_ShouldCloseDialogWithoutFaultingTask()
    {
        // Arrange
        const int content = 42;
        var service = new AvaloniaDialogService();
        var window = new Window();
        var builder = new FluentAvaloniaContentDialogBuilder<int>(service, "Title", content, window);

        // Act
        window.Show();
        Dispatcher.UIThread.RunJobs();
        DialogAwaitable<int> dialog = builder.ShowAsync();
        dialog.Dispose();
        ContentDialogResult<int> result = await dialog;

        // Assert
        result.Result.ShouldBe(ContentDialogResult.None);
        result.Content.ShouldBe(content);
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
        builder.ShouldBeEquivalentTo(returnedBuilder);
        builder.Dialog.SecondaryButtonText.ShouldBe(buttonText);
    }

    [Fact]
    public void SetSecondaryButton_IsEnabled_ShouldBeBoundCorrectly()
    {
        // Arrange
        const string buttonText = "ButtonText";
        var service = new AvaloniaDialogService();
        FluentAvaloniaContentDialogBuilder<int> builder = Dispatcher.UIThread.Invoke(() =>
            new FluentAvaloniaContentDialogBuilder<int>(service, "Title", 42)
        );
        var subject = new BehaviorSubject<bool>(false);

        // Act and assert
        IContentDialogBuilder<int> returnedBuilder = builder.SetSecondaryButton(buttonText, isEnabled: subject);

        builder.ShouldBeEquivalentTo(returnedBuilder);
        builder.Dialog.IsSecondaryButtonEnabled.ShouldBeFalse();

        subject.OnNext(true);
        builder.Dialog.IsSecondaryButtonEnabled.ShouldBeTrue();

        subject.OnNext(false);
        builder.Dialog.IsSecondaryButtonEnabled.ShouldBeFalse();
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
        IContentDialogBuilder<int> returnedBuilder = builder.SetSecondaryButton(
            buttonText,
            onClick: (c, _) =>
            {
                contentReceivedOnClick = c;
                return Task.FromResult(true);
            }
        );
        builder.Dialog.Opened += (sender, _) =>
        {
            Button button = sender.GetFirstDescendant<Button>("SecondaryButton");
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            window.MouseClick(button.GetRelativeBounds(window).Center, MouseButton.Left);
        };
        ContentDialogResult<int> result = await builder.ShowAsync();

        // Assert
        builder.ShouldBeEquivalentTo(returnedBuilder);
        contentReceivedOnClick.ShouldBe(content);
        result.Result.ShouldBe(ContentDialogResult.Secondary);
        result.Content.ShouldBe(content);
        window.Close();
    }

    [AvaloniaTheory(Timeout = 5000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MessageBoxModel_ShowAsync_ShouldHaveValidTemplates(bool isSelectable)
    {
        // Arrange
        const string messageText = "message";
        var content = new MessageBoxViewModel { Message = messageText, IsSelectable = isSelectable };
        var service = new AvaloniaDialogService();
        var window = new Window();
        var builder = new FluentAvaloniaContentDialogBuilder<MessageBoxViewModel>(service, "Title", content, window);

        // Act
        window.Show();
        Dispatcher.UIThread.RunJobs();
        bool? isSelectableTextBlock = null;
        builder.Dialog.Opened += (sender, _) =>
        {
            TextBlock? textBlock = sender
                .GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(x => x.IsVisible && x.Text == messageText);
            if (textBlock is not null)
            {
                isSelectableTextBlock = false;
            }
            SelectableTextBlock? selectableTextBlock = sender
                .GetVisualDescendants()
                .OfType<SelectableTextBlock>()
                .FirstOrDefault(x => x.IsVisible && x.Text == messageText);
            if (selectableTextBlock is not null)
            {
                isSelectableTextBlock = true;
            }
            builder.Dialog.Hide();
        };
        ContentDialogResult<MessageBoxViewModel> result = await builder.ShowAsync();

        // Assert
        isSelectableTextBlock.ShouldBe(isSelectable);
        result.Result.ShouldBe(ContentDialogResult.None);
        window.Close();
    }
}

file static class Extensions
{
    public static int GetRegisteredDisposableCount(this AvaloniaDialogService service)
    {
        FieldInfo? field = typeof(AvaloniaDialogService).GetField(
            "_disposables",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        if (field?.GetValue(service) is not ICollection disposables)
            return 0;
        return disposables.Count;
    }

    public static global::FluentAvalonia.UI.Controls.FAContentDialogButtonClickEventArgs InvokePrimaryButtonClick(
        global::FluentAvalonia.UI.Controls.FAContentDialog dialog
    )
    {
        FAContentDialogButtonClickEventArgs args =
            (global::FluentAvalonia.UI.Controls.FAContentDialogButtonClickEventArgs?)
                Activator.CreateInstance(
                    typeof(global::FluentAvalonia.UI.Controls.FAContentDialogButtonClickEventArgs),
                    nonPublic: true
                )
            ?? throw new InvalidOperationException("Could not create primary button click event args");
        MethodInfo method =
            typeof(global::FluentAvalonia.UI.Controls.FAContentDialog).GetMethod(
                "OnPrimaryButtonClick",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            ) ?? throw new InvalidOperationException("Could not find primary button click handler");
        method.Invoke(dialog, [args]);
        return args;
    }

    public static T GetFirstDescendant<T>(this Visual visual, string name)
        where T : StyledElement => visual.GetVisualDescendants().GetFirstDescendant<T>(name);

    public static T GetFirstDescendant<T>(this IEnumerable<Visual> source, string name)
        where T : StyledElement => source.OfType<T>().First(x => x.Name == name);

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
        Matrix? transform =
            visual.TransformToVisual(second)
            ?? throw new ArgumentOutOfRangeException(
                null,
                "Could not calculate transformation from one visual to the other"
            );
        return new Rect(visual.Bounds.Size).TransformToAABB(transform.Value);
    }

    /// <summary> Simulates a mouse click (down and up) on the headless window/toplevel. </summary>
    public static void MouseClick(
        this TopLevel topLevel,
        Point point,
        MouseButton button,
        RawInputModifiers modifiers = RawInputModifiers.None
    )
    {
        topLevel.MouseDown(point, button, modifiers);
        topLevel.MouseUp(point, button, modifiers);
    }
}
