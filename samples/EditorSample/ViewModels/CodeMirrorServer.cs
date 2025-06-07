namespace EditorSample.ViewModels;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using MirrorSharp;
using MirrorSharp.Advanced;
using MirrorSharp.AspNetCore;

public sealed class CodeMirrorService : INotifyPropertyChanged, IAsyncDisposable
{
    private WebApplication? _webApplication;

    public string Address
    {
        get;
        private set => SetField(ref field, value);
    } = string.Empty;

    public bool IsRunning
    {
        get;
        private set => SetField(ref field, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary> Start the backend server. Completion of the returned task indicates that the webservice was started successfully </summary>
    /// <param name="onBuild"> A callback with the WebApplicationBuild. Can be used to register e.g. logging </param>
    /// <param name="onConfigureCSharp"> A callback with the CSharpOptions. Can be used to configure Roslyn </param>
    /// <param name="isDebugLoggingEnabled"> If true, logging in case of errors is enabled </param>
    /// <param name="cancellationToken"> The cancellationToken to cancel the starting operation </param>
    /// <exception cref="InvalidOperationException"> Thrown if the server is not started on a valid address </exception>
    public async Task StartBackendAsync(
        Action<WebApplicationBuilder>? onBuild = null,
        Action<MirrorSharpCSharpOptions>? onConfigureCSharp = null,
        bool isDebugLoggingEnabled = false,
        CancellationToken cancellationToken = default
    )
    {
        WebApplicationBuilder builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddRoutingCore();
        if (isDebugLoggingEnabled)
            builder.Services.AddSingleton<IExceptionLogger, ExceptionLogger>();
        onBuild?.Invoke(builder);

        _webApplication = builder.Build();

        // Serve the embedded Assets/ folder
        var assetsProvider = new EmbeddedFileProvider(typeof(CodeMirrorService).Assembly, "EditorSample.Assets");
        _webApplication.UseStaticFiles(new StaticFileOptions { FileProvider = assetsProvider });

        // MirrorSharp WebSocket endpoint
        _webApplication.UseWebSockets();
        MirrorSharpOptions options = new MirrorSharpOptions().SetupCSharp(o => onConfigureCSharp?.Invoke(o));
        if (isDebugLoggingEnabled)
        {
            options.IncludeExceptionDetails = true;
            options.SelfDebugEnabled = true;
        }
        _webApplication.MapMirrorSharp("/mirrorsharp", options);

        await _webApplication.StartAsync(cancellationToken);

        _webApplication.Lifetime.ApplicationStopping.Register(() => Dispatcher.UIThread.Post(() => IsRunning = false));
        var chosenAddress =
            GetPorts(_webApplication.Services).FirstOrDefault()
            ?? throw new InvalidOperationException("Server not bound to any ports");
        Dispatcher.UIThread.Post(() =>
        {
            Address = $"{chosenAddress}/index.html";
            IsRunning = true;
        });
    }

    private static IEnumerable<string> GetPorts(IServiceProvider provider)
    {
        IServer? server = provider.GetService<IServer>();
        IServerAddressesFeature? addressesFeature = server?.Features.Get<IServerAddressesFeature>();
        return addressesFeature?.Addresses ?? [];
    }

    public async ValueTask DisposeAsync()
    {
        if (_webApplication is null)
            return;
        await _webApplication.DisposeAsync();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

file sealed class ExceptionLogger(ILogger<ExceptionLogger>? logger) : IExceptionLogger
{
    private readonly ILogger<ExceptionLogger>? _logger = logger;

    public void LogException(Exception exception, IWorkSession session)
    {
        if (_logger is null || !_logger.IsEnabled(LogLevel.Error))
            return;
        using IDisposable? _ = _logger.BeginScope(
            new Dictionary<string, object>
            {
                [nameof(IWorkSession.IsRoslyn)] = session.IsRoslyn,
                [nameof(IWorkSession.LanguageName)] = session.LanguageName,
                [nameof(IWorkSession.ExtensionData)] = session.ExtensionData,
            }
        );
        _logger?.LogError(exception, "CodeMirror with exception: {Message}", exception.Message);
    }
}
