[![Build](https://github.com/reactiveui/reactive.wasm/actions/workflows/ci-build.yml/badge.svg)](https://github.com/reactiveui/reactive.wasm/actions/workflows/ci-build.yml)
[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://reactiveui.net/contribute)
[![](https://img.shields.io/badge/chat-slack-blue.svg)](https://reactiveui.net/slack)
[![NuGet](https://img.shields.io/nuget/v/Reactive.Wasm.svg)](https://www.nuget.org/packages/Reactive.Wasm/)

<br>
<a href="https://github.com/reactiveui/reactiveui">
  <img width="160" heigth="160" src="https://raw.githubusercontent.com/reactiveui/styleguide/master/logo/main.png">
</a>
<br>

# System.Reactive for WebAssembly

This package provides WebAssembly-specific implementations for [System.Reactive](https://github.com/dotnet/reactive), designed to be integrated into ReactiveUI and ReactiveUI.Uno projects to enable optimal reactive programming in WebAssembly environments. It provides platform-aware scheduler selection and WASM-optimized concurrency abstractions for cross-platform applications targeting Blazor WebAssembly and other WASM hosts.

---
## NuGet Packages

To get started, install the following package into your WebAssembly project.

| Platform          | NuGet                  |
| ----------------- | ---------------------- |
| WebAssembly       | [![NuGet](https://img.shields.io/nuget/v/Reactive.Wasm.svg)](https://www.nuget.org/packages/Reactive.Wasm/)     |

-----

## Tutorial: Integrating with ReactiveUI Projects

Welcome to the `System.Reactive.Wasm` integration guide! This tutorial shows you how to integrate this package into ReactiveUI and ReactiveUI.Uno projects to enable optimal reactive programming across platforms, with special support for WebAssembly environments like Blazor WebAssembly.

The most common usage of `System.Reactive.Wasm` is not as a standalone library, but as an integration component that provides platform-aware scheduler selection for cross-platform ReactiveUI applications.

### Chapter 1: Basic Integration with ReactiveUI

The primary integration scenario involves configuring ReactiveUI's scheduler selection to use appropriate schedulers based on the target platform.

#### 1. Installation

Add the `Reactive.Wasm` package to your ReactiveUI project that targets WebAssembly:

```xml
<PackageReference Include="Reactive.Wasm" Version="[latest-version]" />
<PackageReference Include="ReactiveUI" Version="[latest-version]" />
```

#### 2. Platform-Aware Scheduler Configuration

Configure your dependency injection to select the appropriate scheduler based on the platform. This is the most common integration pattern:

```csharp
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

public static void ConfigureServices(IServiceCollection services)
{
    // Configure platform-aware scheduler selection
    // WebAssembly does not support Threads for .NET5/.NET6+
    _ = services
        .AddSingleton<IScheduler>(static provider =>
        {
            return provider.GetRequiredService<Platform>() switch
            {
                Platform.WebAssembly => RxApp.MainThreadScheduler,
                _ => RxApp.TaskpoolScheduler,
            };
        });

    // Register your ViewModels and other services
    services.AddTransient<MyViewModel>();
}
```

#### 3. Platform Detection

Create a simple platform detection service to identify WebAssembly environments:

```csharp
public enum Platform
{
    WebAssembly,
    Desktop,
    Mobile
}

public interface IPlatformService
{
    Platform CurrentPlatform { get; }
}

public class PlatformService : IPlatformService
{
    public Platform CurrentPlatform => DetectPlatform();

    private static Platform DetectPlatform()
    {
#if WEBASSEMBLY
        return Platform.WebAssembly;
#elif ANDROID || IOS
        return Platform.Mobile;
#else
        return Platform.Desktop;
#endif
    }
}
```

### Chapter 2: ReactiveUI.Uno Integration

For Uno Platform projects using ReactiveUI, the integration follows a similar pattern with some Uno-specific considerations.

#### 1. Uno Platform Project Setup

In your Uno Platform project, configure the scheduler selection in your `App.xaml.cs`:

```csharp
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Platform-aware scheduler configuration for Uno
        services.AddSingleton<IScheduler>(provider =>
        {
#if __WASM__
            // WebAssembly platform uses MainThreadScheduler
            return RxApp.MainThreadScheduler;
#else
            // Other Uno platforms use TaskpoolScheduler
            return RxApp.TaskpoolScheduler;
#endif
        });

        // Register ReactiveUI services
        services.AddSingleton<IViewLocator, DefaultViewLocator>();

        // Register your ViewModels
        services.AddTransient<MainViewModel>();
    }
}
```

#### 2. ViewModel Integration

Your ViewModels work seamlessly across all Uno platforms with the configured schedulers:

```csharp
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

public class MainViewModel : ReactiveObject
{
    private string _message = "Hello Uno!";

    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public ReactiveCommand<Unit, Unit> UpdateMessageCommand { get; }

    public MainViewModel()
    {
        UpdateMessageCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // This will use the appropriate scheduler based on platform
            Message = $"Updated at {DateTime.Now:HH:mm:ss}";

            // Demonstrate reactive stream that works across platforms
            await Observable.Timer(TimeSpan.FromSeconds(1))
                .Select(_ => $"Delayed update at {DateTime.Now:HH:mm:ss}")
                .Do(msg => Message = msg);
        });
    }
}
```

## Advanced Usage

The following sections cover advanced scenarios where you need direct access to WASM-specific reactive features. Most applications using ReactiveUI or ReactiveUI.Uno won't need these advanced techniques.

### Direct WASM Scheduler Usage

The `WasmScheduler` is specifically designed to work efficiently with the WebAssembly runtime, providing optimal scheduling for both immediate and delayed operations.

#### 1. Using the WASM Scheduler Directly

```csharp
using System.Reactive.Concurrency;

public class SchedulerExample
{
    public void DemonstrateWasmScheduler()
    {
        var scheduler = WasmScheduler.Default;

        // Schedule immediate work
        scheduler.Schedule(() => Console.WriteLine("Immediate work executed"));

        // Schedule delayed work
        scheduler.Schedule(TimeSpan.FromSeconds(2), () =>
            Console.WriteLine("Delayed work executed after 2 seconds"));

        // Schedule periodic work
        var periodicWork = scheduler.SchedulePeriodic(TimeSpan.FromSeconds(1), () =>
            Console.WriteLine($"Periodic work executed at {DateTime.Now:HH:mm:ss}"));
    }
}
```

#### 2. Reactive Streams with Custom Scheduling

```csharp
using System.Reactive.Linq;
using System.Reactive.Concurrency;

public class CustomSchedulingExample
{
    public void CreateScheduledStream()
    {
        var wasmScheduler = WasmScheduler.Default;

        // Create an observable that uses the WASM scheduler
        var scheduledStream = Observable.Generate(
            0,                           // Initial state
            x => x < 10,                // Continue condition
            x => x + 1,                 // Iterator
            x => $"Value: {x}",        // Result selector
            x => TimeSpan.FromMilliseconds(500), // Time selector
            wasmScheduler)              // Use WASM scheduler
            .Subscribe(value => Console.WriteLine(value));
    }
}
```

### Platform Enlightenment Provider (Advanced)

For scenarios where you need manual control over the reactive platform services, you can use the Platform Enlightenment Provider directly:

```csharp
using System.Reactive.PlatformServices;

public static void Main(string[] args)
{
    // Enable WASM-specific reactive extensions
#pragma warning disable CS0618 // Type or member is obsolete
    PlatformEnlightenmentProvider.Current.EnableWasm();
#pragma warning restore CS0618 // Type or member is obsolete

    // ... rest of your application initialization
}
```

### Performance Optimization Techniques

WebAssembly has unique performance characteristics. Here are some best practices for reactive programming in WASM environments.

#### 1. Efficient Observable Chains

```csharp
public class PerformanceOptimizedExample
{
    public IObservable<string> CreateOptimizedStream(IObservable<int> source)
    {
        return source
            .Where(x => x > 0)           // Filter early
            .Buffer(TimeSpan.FromMilliseconds(100)) // Batch operations
            .Where(buffer => buffer.Count > 0)      // Avoid empty batches
            .Select(buffer => $"Processed {buffer.Count} items")
            .DistinctUntilChanged()      // Avoid duplicate notifications
            .ObserveOn(WasmScheduler.Default); // Ensure WASM scheduling
    }
}
```

#### 2. Memory Management in WASM

```csharp
using System.Reactive.Disposables;

public class MemoryEfficientExample : IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public void SetupReactiveStreams()
    {
        // Always dispose of subscriptions in WASM to prevent memory leaks
        var subscription1 = Observable.Timer(TimeSpan.FromSeconds(1))
            .Subscribe(x => Console.WriteLine($"Timer: {x}"));

        var subscription2 = Observable.FromEvent<EventArgs>(
            h => SomeEvent += h,
            h => SomeEvent -= h)
            .Subscribe(x => Console.WriteLine("Event received"));

        // Add to composite disposable for easy cleanup
        _disposables.Add(subscription1);
        _disposables.Add(subscription2);
    }

    public event EventHandler<EventArgs>? SomeEvent;

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
```

### Direct Framework Integration (Advanced)

System.Reactive.Wasm works seamlessly with various WASM frameworks.

#### 1. Blazor WebAssembly Integration

```csharp
@page "/reactive-demo"
@using System.Reactive.Linq
@using System.Reactive.Subjects
@implements IDisposable

<h3>Reactive WASM Demo</h3>
<p>Current Time: @currentTime</p>
<p>Counter: @counter</p>

@code {
    private string currentTime = "";
    private int counter = 0;
    private IDisposable? timeSubscription;
    private IDisposable? counterSubscription;

    protected override void OnInitialized()
    {
        // Create a time stream
        timeSubscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Select(_ => DateTime.Now.ToString("HH:mm:ss"))
            .Subscribe(time =>
            {
                currentTime = time;
                InvokeAsync(StateHasChanged);
            });

        // Create a counter stream
        counterSubscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(100))
            .Subscribe(tick =>
            {
                counter = (int)tick;
                InvokeAsync(StateHasChanged);
            });
    }

    public void Dispose()
    {
        timeSubscription?.Dispose();
        counterSubscription?.Dispose();
    }
}
```

This tutorial has covered the basics of getting started with `System.Reactive.Wasm`. You've learned how to set up your project, use the WASM scheduler, optimize performance, and integrate with WASM frameworks. From here, you can explore more advanced System.Reactive features, all optimized for WebAssembly environments.

-----

## Thanks

We want to thank the following contributors and libraries that help make System.Reactive.Wasm possible:

### Core Libraries

  - **System.Reactive**: [Reactive Extensions for .NET](https://github.com/dotnet/reactive) - The foundation of reactive programming in .NET.
  - **Splat**: [Splat](https://github.com/reactiveui/splat) - Cross-platform utilities and service location.
  - **Mono Runtime**: The Mono WebAssembly runtime that enables .NET in WebAssembly environments.
  - **Uno.Rx.NET**: [Uno.Rx.NET](https://github.com/nventive/Uno.Rx.NET) - Original implementation that inspired this project.

-----

## Sponsorship

The core team members, ReactiveUI contributors and contributors in the ecosystem do this open-source work in their free time. If you use System.Reactive.Wasm, a serious task, and you'd like us to invest more time on it, please donate. This project increases your income/productivity too. It makes development and applications faster and it reduces the required bandwidth.

[Become a sponsor](https://github.com/sponsors/reactivemarbles).

This is how we use the donations:

  * Allow the core team to work on System.Reactive.Wasm
  * Thank contributors if they invested a large amount of time in contributing
  * Support projects in the ecosystem

-----

## Support

If you have a question, please see if any discussions in our [GitHub Discussions](https://github.com/reactiveui/reactive.wasm/discussions) or [GitHub issues](https://github.com/reactiveui/reactive.wasm/issues) have already answered it.

If you want to discuss something or just need help, here is our [Slack room](https://reactiveui.net/slack), where there are always individuals looking to help out!

Please do not open GitHub issues for support requests.

-----

## Contribute

System.Reactive.Wasm is developed under an OSI-approved open source license, making it freely usable and distributable, even for commercial use.

If you want to submit pull requests please first open a [GitHub issue](https://github.com/reactiveui/reactive.wasm/issues/new/choose) to discuss. We are first time PR contributors friendly.

See [Contribution Guidelines](https://www.reactiveui.net/contribute/) for further information how to contribute changes.

-----

## License

System.Reactive.Wasm is licensed under the [MIT License](https://github.com/reactiveui/reactive.wasm/blob/main/LICENSE).
