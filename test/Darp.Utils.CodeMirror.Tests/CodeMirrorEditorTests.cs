using System.Diagnostics;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(Darp.Utils.CodeMirror.Tests.TestAppBuilder))]

namespace Darp.Utils.CodeMirror.Tests;

public class TestApp : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
    }
}

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public class CodeMirrorEditorTests : IAsyncLifetime
{
    private readonly CodeMirrorEditor _editor;
    private readonly ICodeMirrorService _service;
    private readonly TaskCompletionSource _editorLoaded = new();

    public CodeMirrorEditorTests()
    {
        _editor = new CodeMirrorEditor();
        _service = new CodeMirrorService();

        _editor.Navigated += (url, _) =>
        {
            if (url.EndsWith("index.html", StringComparison.InvariantCulture))
            {
                _editorLoaded.SetResult();
            }
        };
    }

    public async Task InitializeAsync()
    {
        await _service.StartBackendAsync(
            onBuild: builder =>
            {
                builder.Logging.AddConsole();
            },
            isDebugLoggingEnabled: true
        );

        _editor.Address = _service.Address;
        await _editorLoaded.Task;

        // Wait for editor to be fully initialized
        await WaitForEditorInitialization();
    }

    public async Task DisposeAsync()
    {
        await _service.DisposeAsync();
    }

    private async Task WaitForEditorInitialization()
    {
        var maxAttempts = 10;
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                Debug.WriteLine($"[Test] Attempt {i + 1} to check editor JS context...");
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var result = await _editor
                        .EvaluateScript<string>("window.getMsText()")
                        .WithTimeout(TimeSpan.FromSeconds(2));
                    Debug.WriteLine($"[Test] JS context responded: {result}");
                });
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Test] Editor JS context not ready: {ex.Message}");
                await Task.Delay(100);
            }
        }
        throw new TimeoutException("Editor failed to initialize");
    }

    [AvaloniaFact(Timeout = 10000)]
    public async Task Editor_WhenTextChanged_ShouldUpdateJavaScriptState()
    {
        // Arrange
        const string testText = "Hello, World!";

        // Act
        await Dispatcher.UIThread.InvokeAsync(() => _editor.EditorText = testText);

        // Wait for the text to be updated in JavaScript
        var jsText = await WaitForCondition(
            async () =>
                await Dispatcher.UIThread.InvokeAsync(
                    async () => await _editor.EvaluateScript<string>("window.getMsText()")
                ),
            text => text == testText
        );

        // Assert
        jsText.ShouldBe(testText);
    }

    [AvaloniaFact(Timeout = 10000)]
    public async Task Editor_WhenReadOnlyChanged_ShouldUpdateJavaScriptState()
    {
        // Act
        await Dispatcher.UIThread.InvokeAsync(() => _editor.IsEditorReadOnly = true);

        // Wait for the read-only state to be updated in JavaScript
        var jsReadOnly = await WaitForCondition(
            async () =>
                await Dispatcher.UIThread.InvokeAsync(
                    async () => await _editor.EvaluateScript<bool>("window.getMsIsReadOnly()")
                ),
            isReadOnly => isReadOnly
        );

        // Assert
        jsReadOnly.ShouldBeTrue();
    }

    [AvaloniaFact(Timeout = 10000)]
    public async Task Editor_WhenLanguageChanged_ShouldUpdateJavaScriptState()
    {
        // Act
        await Dispatcher.UIThread.InvokeAsync(() => _editor.EditorLanguage = CodeMirrorLanguage.CSharp);

        // Wait for the language to be updated in JavaScript
        var jsLanguage = await WaitForCondition(
            async () =>
                await Dispatcher.UIThread.InvokeAsync(
                    async () => await _editor.EvaluateScript<string>("window.getMsLanguage()")
                ),
            language => language == "C#"
        );

        // Assert
        jsLanguage.ShouldBe("C#");
    }

    [AvaloniaFact(Timeout = 10000)]
    public async Task Editor_WhenThemeChanged_ShouldUpdateJavaScriptState()
    {
        // Arrange
        var app = Application.Current;
        if (app == null)
            throw new InvalidOperationException("Application.Current is null");

        // Act
        await Dispatcher.UIThread.InvokeAsync(() => app.RequestedThemeVariant = ThemeVariant.Dark);

        // Wait for the theme to be updated in JavaScript
        var jsTheme = await WaitForCondition(
            async () =>
                await Dispatcher.UIThread.InvokeAsync(
                    async () => await _editor.EvaluateScript<string>("window.getMsTheme()")
                ),
            theme => theme == "dark"
        );

        // Assert
        jsTheme.ShouldBe("dark");
    }

    [AvaloniaFact(Timeout = 10000)]
    public async Task Editor_WhenTextChangedInJavaScript_ShouldUpdateCSharpState()
    {
        // Arrange
        const string testText = "Hello from JavaScript!";

        // Act
        await Dispatcher.UIThread.InvokeAsync(
            async () => await _editor.EvaluateScript<string>($"""window.setMsText("{testText}");""")
        );

        // Wait for the text to be updated in C#
        var csharpText = await WaitForCondition(
            async () => await Dispatcher.UIThread.InvokeAsync(() => Task.FromResult(_editor.EditorText)),
            text => text == testText
        );

        // Assert
        csharpText.ShouldBe(testText);
    }

    private async Task<T> WaitForCondition<T>(
        Func<Task<T>> getValue,
        Func<T, bool> condition,
        int maxAttempts = 50,
        int delayMs = 100
    )
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var value = await getValue().WithTimeout(TimeSpan.FromSeconds(2));
                Debug.WriteLine($"[Test] WaitForCondition attempt {i + 1}: {value}");
                if (condition(value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Test] WaitForCondition exception: {ex.Message}");
            }
            await Task.Delay(delayMs);
        }
        throw new TimeoutException($"Condition not met after {maxAttempts} attempts");
    }
}

public static class TaskExtensions
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            return await task;
        throw new TimeoutException($"Task timed out after {timeout.TotalSeconds} seconds");
    }
}
