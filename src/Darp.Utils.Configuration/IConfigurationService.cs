namespace Darp.Utils.Configuration;

using System.ComponentModel;

/// <summary> An obsolete placeholder </summary>
/// <typeparam name="TConfig">The type describing the underlying config</typeparam>
[Obsolete("Use the ConfigService directly, please", true)]
public interface IConfigurationService<TConfig> : IDisposable, INotifyPropertyChanged;
