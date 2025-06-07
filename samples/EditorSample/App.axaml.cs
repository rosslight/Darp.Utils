using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EditorSample.ViewModels;
using EditorSample.Views;

namespace EditorSample;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            var viewModel = new MainWindowViewModel();
            _ = viewModel.CodeMirror.StartBackendAsync(
                onBuild: builder => builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole()),
                options => options.SetScriptMode()
            );
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
