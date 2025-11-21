namespace AvaloniaApp;

using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Darp.Utils.CodeMirror;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.FluentAvalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViewModels;
using Views;

public sealed partial class App : Application
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
                onConfigureCSharp: options =>
                    options
                        .SetScriptMode(true, typeof(Globals))
                        .SetMetadataReferencesFromAssembly(
                            Assembly.Load("System.Runtime"),
                            Assembly.Load("netstandard"),
                            typeof(Globals).Assembly,
                            typeof(object).Assembly
                        )
                        .AddUsings("System")
            );
            desktop.MainWindow = new MainWindow { ViewModel = vm };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public class Globals(string str = "Hello!")
{
    public string SomeStaticString { get; } = str;
    public int myInt = 123;
}
