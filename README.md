# Darp.Utils

[![Test (and publish)](https://github.com/rosslight/Darp.Utils/actions/workflows/test_and_publish.yml/badge.svg)](https://github.com/rosslight/Darp.Utils/actions/workflows/test_and_publish.yml)
![License](https://img.shields.io/badge/license-Apache2-0)

This repository bundles all open source c# helper modules of 'rosslight GmbH'.
To extend, add a new project and test project.

## Darp.Utils.Assets
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.Assets.svg)](https://www.nuget.org/packages/Darp.Utils.Assets)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.Assets)](https://www.nuget.org/packages/Darp.Utils.Assets)

A collection of simple interfaces for app assets targeting desktop apps.

Currently implemented:
- `AppDataAssetsService`: Read or write to the `ApplicationData`. The relativePath might be e.g. the app name.

Example:
```csharp
ServiceProvider provider = new ServiceCollection()
    .AddAppDataAssetsService("RelativePath")
    .BuildServiceProvider();

IAppDataAssetsService service = provider.GetRequiredService<IAppDataAssetsService>();
await service.SerializeJsonAsync("test.json", new { Prop1 = "value" });
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
    .AddConfigurationFile<TestConfig, IAppDataAssetsService>("config.json")
    .BuildServiceProvider();

IConfigurationService<TestConfig> service = provider.GetRequiredService<IConfigurationService<TestConfig>>();
TestConfig config = await service.LoadConfigurationAsync();
await service.WriteConfigurationAsync(config with { Setting = "NewValue" });
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
await _dialogService.CreateMessageBoxDialog("Title", "Message").ShowAsync();

// Assumes you have registered a view for 'SomeViewModel' in a ViewLocator
// Works with any kind of content
var viewModel = new SomeViewModel();
await _dialogService.CreateContentDialog("Title", viewModel)
    .SetDefaultButton(ContentDialogButton.Primary)
    .SetCloseButton("Close")
    .SetPrimaryButton("Ok", onClick: model => model.IsModelValid)
    .ShowAsync();
```

## Darp.Utils.ResxSourceGenerator
[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.ResxSourceGenerator.svg)](https://www.nuget.org/packages/Darp.Utils.ResxSourceGenerator)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.ResxSourceGenerator)](https://www.nuget.org/packages/Darp.Utils.ResxSourceGenerator)

A source generator for generating strongly typed singleton resource classes from .resx files.
Additional documentation [here](https://github.com/rosslight/Darp.Utils/tree/main/src/Darp.Utils.ResxSourceGenerator/README.md).
