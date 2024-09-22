namespace Darp.Utils.Dialog.FluentAvalonia.Tests;

using Avalonia;
using Avalonia.Headless;
using global::FluentAvalonia.Styling;

public class TestApp : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentAvaloniaTheme());
        base.OnFrameworkInitializationCompleted();
    }
}

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

