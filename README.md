Enables Rx extensions in web-assembly projects.
Based on [Uno.Rx.NET](https://github.com/nventive/Uno.Rx.NET).

A reference to the [`Reactive.Wasm`](https://www.nuget.org/packages/Reactive.Wasm) NuGet package should be added, and then it must be manually enabled, preferably in the entry point of your app (i.e. at the beginning of the `Main` method in your WASM project), call the following:

```c#
#pragma warning disable CS0618 // Type or member is obsolete
      PlatformEnlightenmentProvider.Current.EnableWasm();
#pragma warning restore CS0618 // Type or member is obsolete
```
