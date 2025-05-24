namespace Darp.Utils.Avalonia;

using System.Diagnostics.CodeAnalysis;
using global::Avalonia.Data.Core.Plugins;

/// <summary> A collection of helpful methods when working with avalonia </summary>
public static class AvaloniaHelpers
{
    /// <summary> Avoid duplicate validations from both Avalonia and the CommunityToolkit. </summary>
    /// <seealso href="https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins"/>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "We are only removing validators")]
    public static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }

    /// <summary> Cast the DataContext to a specific ViewModel Type </summary>
    /// <param name="dataContext"> The DataContext </param>
    /// <typeparam name="TViewModel"> The type of the ViewModel </typeparam>
    /// <returns> The ViewModel </returns>
    /// <exception cref="InvalidOperationException"> Thrown if the DataContext is null or not the ViewModel </exception>
    internal static TViewModel GetViewModel<TViewModel>([NotNull] object? dataContext)
    {
        if (dataContext is null)
            throw new InvalidOperationException("Cannot retrieve ViewModel. The DataContext is null");

        if (dataContext is not TViewModel viewModel)
        {
            throw new InvalidOperationException(
                $"Cannot retrieve ViewModel. The DataContext is has an invalid type. Expected {typeof(TViewModel).Name} but is {dataContext.GetType().Name}"
            );
        }

        return viewModel;
    }
}
