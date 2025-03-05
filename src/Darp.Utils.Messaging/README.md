<div align="center">

# Darp.Utils.Messaging

[![NuGet](https://img.shields.io/nuget/v/Darp.Utils.Messaging.svg)](https://www.nuget.org/packages/Darp.Utils.Messaging)
[![Downloads](https://img.shields.io/nuget/dt/Darp.Utils.Messaging)](https://www.nuget.org/packages/Darp.Utils.Messaging)

![Dotnet Version](https://img.shields.io/badge/dotnet-netstandard2.0%20%7C%20net9.0-blue)
![Language Version](https://img.shields.io/badge/c%23-11-blue)

[![Tests](https://github.com/rosslight/Darp.Utils/actions/workflows/test_and_publish.yml/badge.svg)](https://github.com/rosslight/Darp.Utils/actions/workflows/test_and_publish.yml)
![License](https://img.shields.io/github/license/rosslight/Darp.Utils)

### A source generator to generate implementation for MessageSource and MessageSinks.

</div>

You should use this packet if you want
- Events that can be subscribed to by a specific type
- Support for ref-like messages (>= net9.0)
- a very slim library

This package is not for you
- need async handling
- do not want source generation
- additional features

## Usage

The library is structured in two core roles:
- `MessageSource`: A class which holds a list of subscribers that can be notified about a new message
- `MessageSink`: A class which holds one or multiple handlers for specific message types

This library generates necessary boilerplate for those two roles and provides a few additional helpers.

### MessageSource

A message source can be defined by adding the `MessageSource` attribute to a partial class.
The source generator will generate a list of subscribers, a `Subscribe` method as well as a protected `Publish` method which can be used to notify subscribers about a new value

```csharp
using Darp.Utils.Messaging;

[MessageSource]
public sealed partial class TestMessageSource
{
    public void PublishInt()
    {
        Publish(42);
    }
}
```

### MessageSink

A message sink can be defined by adding a `MessageSink` attribute to any void method with only a single parameter.
The source generator will generate a class which can be used to subscribe to any source and links the chosen method to those events.

```csharp
using Darp.Utils.Messaging;

public sealed partial class TestClass
{
    [MessageSink]
    private void OnInt(int message) { }

    [MessageSink]
    private void OnSpan(System.ReadOnlySpan<byte> message) { }

    [MessageSink]
    private void OnAny<T>(T message) where T : allows ref struct { } // Add 'allows ref struct' only for >= net9.0
}
```

### Helpers

A few helpers are predefined in the library. If you do not want to define a new class you can use the `MessageSubject`.
Also, it is possible to Subscribe for a certain type directly or convert the source to an `IObservable`:

```csharp
using Darp.Utils.Messaging;

var subject = new MessageSubject();

// Subscribe to any source and use the dispose method to unsubscribe
IDisposable disposable = source.Subscribe<int>(_ => {});
source.Subscribe<string>(_ => {});

// Convert to an IObservable and use with reactive overloads
IObservable observable = source.AsObservable<int>();

// Publish new values
subject.Publish("Some string");
subject.Publish(42);
```

You can take a look at the tests at `test/Darp.Utils.Messaging.Generator.Verify` for additional usage examples.
Also, you can take a look at the expected generated code there.
