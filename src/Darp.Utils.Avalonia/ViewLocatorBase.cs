namespace Darp.Utils.Avalonia;

using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;

/// <summary> A base-class for resolving views from viewModels </summary>
public abstract class ViewLocatorBase<TViewModelBase> : IDataTemplate
    where TViewModelBase : IViewModelBase
{
    /// <inheritdoc />
    public Control Build(object? param)
    {
        if (param is not TViewModelBase viewModel)
            return new TextBlock { Text = $"No VM provided: {param?.GetType()}" };
        return Build(viewModel) ?? new TextBlock { Text = $"VM Not Registered: {viewModel.GetType()}" };
    }

    /// <summary> Resolve the ViewModel to the corresponding view </summary>
    /// <param name="viewModel"> The viewModel to be resolved </param>
    /// <returns> The view corresponding to the ViewModel </returns>
    /// <example>
    /// Overwrite the build method and resolve the correct view. If no view was found, return <c>null</c>
    /// <code>
    /// protected override Control? Build(ViewModelBase viewModel) => viewModel switch
    /// {
    ///     WelcomeViewModel vm => new WelcomeView { ViewModel = vm },
    ///     SettingsViewModel vm => new SettingsView { ViewModel = vm },
    ///     _ => null,
    /// };
    /// </code>
    /// </example>
    protected abstract Control? Build(TViewModelBase viewModel);

    /// <inheritdoc />
    public bool Match(object? data) => data is TViewModelBase;
}
