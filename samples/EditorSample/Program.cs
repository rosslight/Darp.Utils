using System;
using Avalonia;

namespace EditorSample;

using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using MirrorSharp;
using MirrorSharp.AspNetCore;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var port = 5000; // NetworkUtils.FindFreePort();
        BackendInfo.Port = port;

        // 2. Spin up Kestrel + MirrorSharp in the background
        _ = Task.Run(() => Backend.StartAsync(port));
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
}

// BackendInfo.cs
internal static class BackendInfo
{
    public static int Port;
}

internal static class Backend
{
    public static async Task StartAsync(int port)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://localhost:{port}");
        builder.Services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace));
        Console.WriteLine(port);

        var app = builder.Build();

        // Serve the embedded Assets/ folder
        var assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(assetsPath) });

        // MirrorSharp WebSocket endpoint
        app.UseWebSockets();
        app.MapMirrorSharp("/mirrorsharp", new MirrorSharpOptions().SetupCSharp(o => o.SetScriptMode()));

        await app.RunAsync();
    }
}

internal static class NetworkUtils
{
    public static int FindFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
