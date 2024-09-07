namespace Darp.Utils.Tests.Dialog;

using Avalonia.Headless;
using Avalonia;
using FluentAvalonia.Styling;

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
    public static Avalonia.AppBuilder BuildAvaloniaApp() => Avalonia.AppBuilder.Configure<TestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

