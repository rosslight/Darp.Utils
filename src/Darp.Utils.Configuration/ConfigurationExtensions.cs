namespace Darp.Utils.Configuration;

using System.ComponentModel;

/// <summary> Extensions for <see cref="IConfigurationService{TConfig}"/> </summary>
public static class ConfigurationServiceExtensions
{
    /// <summary> Observe changes to a specific configuration value </summary>
    /// <param name="configurationService"> The configuration service to observe </param>
    /// <param name="valueSelector"> The selector of the configuration value </param>
    /// <typeparam name="TConfig"> The type of the configuration </typeparam>
    /// <typeparam name="T"> The type of the value </typeparam>
    /// <returns> An observable with new values when the configuration has changed </returns>
    public static IObservable<T> Observe<TConfig, T>(
        this IConfigurationService<TConfig> configurationService,
        Func<TConfig, T> valueSelector
    ) => new ConfigurationObservable<TConfig, T>(configurationService, valueSelector);
}

file sealed class ConfigurationObservable<TConfig, T>(
    IConfigurationService<TConfig> configurationService,
    Func<TConfig, T> valueSelector
) : IObservable<T>
{
    private readonly IConfigurationService<TConfig> _configurationService = configurationService;
    private readonly Func<TConfig, T> _valueSelector = valueSelector;
    private T? _currentValue;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        PropertyChangedEventHandler handler = GetConfigChangedHandler(observer);
        _configurationService.PropertyChanged += handler;
        return new Disposable(() => _configurationService.PropertyChanged -= handler);
    }

    private PropertyChangedEventHandler GetConfigChangedHandler(IObserver<T> observer) =>
        (_, args) =>
        {
            if (args.PropertyName is not nameof(IConfigurationService<>.Config))
                return;
            T newValue = _valueSelector(_configurationService.Config);
            if (!EqualityComparer<T>.Default.Equals(_currentValue, newValue))
                return;
            observer.OnNext(newValue);
            _currentValue = newValue;
        };
}

file sealed class Disposable(Action onDispose) : IDisposable
{
    private readonly Action _onDispose = onDispose;

    public void Dispose() => _onDispose();
}
