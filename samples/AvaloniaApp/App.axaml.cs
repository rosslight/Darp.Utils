namespace AvaloniaApp;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.FluentAvalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViewModels;
using Views;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ServiceProvider provider = new ServiceCollection()
                .AddTransient<MainWindowViewModel>()
                .AddSingleton<IDialogService>(_ => new AvaloniaDialogService())
                .BuildServiceProvider();
            MainWindowViewModel vm = provider.GetRequiredService<MainWindowViewModel>();
            _ = vm.CodeMirror.StartBackendAsync(
                onBuild: builder => builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole()),
                onConfigureCSharp: options => options.SetScriptMode()
            );
            desktop.MainWindow = new MainWindow { ViewModel = vm };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public sealed class Globals
{
    public string SomeStaticString = "Hello!";
}
