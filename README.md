# Darp.Utils

[![Test (and publish)](https://github.com/rosslight/Darp.Utils/actions/workflows/test_and_publish.yml/badge.svg)](https://github.com/rosslight/Darp.Utils/actions/workflows/test_and_publish.yml)
![License](https://img.shields.io/github/license/rosslight/Darp.Utils)

This repository bundles all open source c# helper modules of 'rosslight GmbH'.
To extend, add a new project and test project.

## Darp.Utils.Assets
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.Assets.svg)](https://www.nuget.org/packages/Darp.Utils.Assets)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.Assets)](https://www.nuget.org/packages/Darp.Utils.Assets)

A collection of simple interfaces for app assets targeting desktop apps.

Currently implemented:
- `FolderAssetsService`: Read or write to a specific folder
  DI extensions provide helpers for `ApplicationData`, `ProgramData`, and the App's BaseDirectory
- `EmbeddedResourceAssetsService`: Read files marked as `EmbeddedResource` of a specific Assembly
- `MemoryAssetsService`: InMemory service that can be used for testing

When using DI, it is possible to retrieve the following services:
- `IAssetsFactory`: A factory which is able to retrieve named asset services
- `IReadOnlyAssets`: The base interface which provides readonly access to your assets
- `IAssetsService`: A writable view on your assets, extends `IReadOnlyAssets`
- `IFolderAssetsService`: A writable view on your assets with helpers specific to a directory

Named asset services:
- Supports registration of multiple different asset services
- Retrieval via `IAssetFactory` by supplying the name

Example:
```csharp
// Add EmbeddedResources and AppData assets to the DI Container
ServiceProvider provider = new ServiceCollection()
    .AddAppDataAssetsService(relativePath: "RelativePath")
    .AddEmbeddedResourceAssetsService(name: "AssemblyResources", typeof(Test).Assembly)
    .BuildServiceProvider();

// Example read and write operations with the app data
IAppDataAssetsService service = provider.GetRequiredService<IAppDataAssetsService>();
await service.SerializeJsonAsync("test.json", new Test("value"));
Test deserialized = await service.DeserializeJsonAsync<Test>("test.json");
await service.WriteTextAsync("test2.txt", "some content");

// Retrieve a named assets service and copy an embedded resource to the app data
IAssetsFactory factory = provider.GetRequiredService<IAssetsFactory>();
IReadOnlyAssetsService resourceService = factory.GetReadOnlyAssets("AssemblyResources");
await resourceService.CopyToAsync("test.json", service, "test.json");

file sealed record Test(string Prop1);
```


## Darp.Utils.Configuration
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.Configuration.svg)](https://www.nuget.org/packages/Darp.Utils.Configuration)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.Configuration)](https://www.nuget.org/packages/Darp.Utils.Configuration)

A writable configuration service. Can be registered using DI and injected into target services.
Usage might include reading, writing and listening to changes via the INotifyPropertyChanged interface.

Example:
```csharp
ServiceProvider provider = new ServiceCollection()
    .AddAppDataAssetsService("RelativePath")
    .AddConfigurationFile<TestConfig>("config.json")
    .BuildServiceProvider();

IConfigurationService<TestConfig> service = provider.GetRequiredService<IConfigurationService<TestConfig>>();
TestConfig config = await service.LoadConfigurationAsync();
await service.WriteConfigurationAsync(config with { Setting = "NewValue" });
```

## Darp.Utils.CodeMirror
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.CodeMirror.svg)](https://www.nuget.org/packages/Darp.Utils.CodeMirror)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.CodeMirror)](https://www.nuget.org/packages/Darp.Utils.CodeMirror)

A code editor control that supports C# based on a WebView with CodeMirror.

First, create the backend service. It will host a minimal webserver which provides the backend.
Afterwards, you can create the editor in the code behind or in xaml:

```csharp
ICodeMirrorService codeMirrorService = new CodeMirrorService();
await vm.CodeMirror.StartBackendAsync(
    onBuild: builder => builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole()),
    onConfigureCSharp: options => options.SetScriptMode(true)
);

var editor = new CodeMirrorEditor();
editor.Address = codeMirrorService.Address;
editor.EditorText = "int i = 42";
editor.IsEditorReadOnly = false;
```

## Darp.Utils.Dialog
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.Dialog.svg)](https://www.nuget.org/packages/Darp.Utils.Dialog)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.Dialog)](https://www.nuget.org/packages/Darp.Utils.Dialog)

A lightweight dialog service which allows for opening dialogs from the ViewModel.

| Implementation                                                                                                                                   | Description                                                                      |
|--------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------|
| [![NuGet](https://img.shields.io/nuget/v/Darp.Utils.Dialog.FluentAvalonia.svg)](https://www.nuget.org/packages/Darp.Utils.Dialog.FluentAvalonia) | Implementation based on [FluentAvalonia](https://github.com/amwx/FluentAvalonia) |

Example:
```csharp
ServiceProvider provider = new ServiceCollection()
    .AddSingleton<IDialogService, AvaloniaDialogService>()
    .BuildServiceProvider();

IDialogService dialogService = provider.GetRequiredService<IDialogService>();
// Specify the Type of the dataContext of the window the dialog is supposed to be shown on
//    .WithDialogRoot<MainWindowViewModel>()
await dialogService.CreateMessageBoxDialog("Title", "Message").ShowAsync();

// Assumes you have registered a view for 'SomeViewModel' in a ViewLocator
// Works with any kind of content
var viewModel = new SomeViewModel();
await dialogService.CreateContentDialog("Title", viewModel)
    .SetDefaultButton(ContentDialogButton.Primary)
    .SetCloseButton("Close")
    .SetPrimaryButton("Ok", onClick: model => model.IsModelValid)
    .ShowAsync();
```

## Darp.Utils.Avalonia
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.Avalonia.svg)](https://www.nuget.org/packages/Darp.Utils.Avalonia)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.Avalonia)](https://www.nuget.org/packages/Darp.Utils.Avalonia)

A collection of classes and methods to help reduce the boilerplate when working with Avalonia. These contain:

- `ViewLocatorBase`: Resolve views at compile-time
- `UserControlBase`, `WindowBase`: Add a `ViewModel` property to have typed access to the DataContext
- `AvaloniaHelpers`: A collection of helper methods

## Darp.Utils.ResxSourceGenerator
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.ResxSourceGenerator.svg)](https://www.nuget.org/packages/Darp.Utils.ResxSourceGenerator)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.ResxSourceGenerator)](https://www.nuget.org/packages/Darp.Utils.ResxSourceGenerator)

A source generator for generating strongly typed singleton resource classes from .resx files.
Additional documentation [here](https://github.com/rosslight/Darp.Utils/tree/main/src/Darp.Utils.ResxSourceGenerator/README.md).

## Darp.Utils.TestRail

[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.TestRail.svg)](https://www.nuget.org/packages/Darp.Utils.TestRail)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.TestRail)](https://www.nuget.org/packages/Darp.Utils.TestRail)

A library allowing for communication with a TestRail instance in a easy and modern way.

Core features:
- Modern: Build on the latest .Net technologies. NativeAot compatible
- Extensible: `ITestRailService` is the core with a bunch of extension methods defining the actual API
- Testable: Operates purely on the interface `ITestRailService` which can be mocked easily

Getting started:
```csharp
var service = TestRailService.Create("https://[your-organization].testrail.io", "username", "passwordOrApiKey");
var projectsEnumerable = service.GetProjects(ProjectsFilter.ActiveProjectsOnly);
await foreach (var project in projectsEnumerable)
{
    var casesEnumerable = service.GetCases(project.Id);
}
var caseResponse = await service.GetCaseAsync((CaseId)1);
var customProperty = caseResponse.Properties["custom_property"].GetString();
await service.UpdateCase(new UpdateCaseRequest { CaseId = caseResponse.Id, Title = "New Title" });
```

Extension methods:
```csharp
public static async Task<GetCaseResponse> GetCaseAsync(this ITestRailService testRailService, CaseId caseId)
{
    var jsonTypeInfo = YourSourceGenerationContext.Default.GetCaseResponse;
    return await testRailService.GetAsync($"/get_case/{(int)caseId}", jsonTypeInfo, default(cancellationToken));
}
```

Usage with `IHttpClientFactory` for http client caching:
```csharp
var provider = new ServiceCollection()
    .AddHttpClient("TestRailClient", (provider, client) =>
        {
            client.BaseAddress = new Uri("https://[your-organization].testrail.io");
            var authBytes = Encoding.UTF8.GetBytes("username:passwordOrApiKey");
            var base64Authorization = Convert.ToBase64String(authBytes);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Authorization);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
    .AddSingleton<ITestRailService>(provider => new TestRailService<IHttpClientFactory>(
        provider.GetRequiredService<IHttpClientFactory>(),
        factory => factory.CreateClient("TestRailClient"),
        NullLogger.Instance))
    .BuildServiceProvider();
var service = provider.GetRequiredService<ITestRailService>();
```
