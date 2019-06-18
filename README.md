Enables Rx extensions in web-assembly projects.
Based on [Uno.Rx.NET](https://github.com/nventive/Uno.Rx.NET).

A reference to the [`Reactive.Wasm`](https://www.nuget.org/packages/Reactive.Wasm) NuGet package should be added, and then it must be manually enabled, preferably in the entry point of your app, call the following (uncomment in multi-platform apps and change directive if necessary):

```c#
//#if __WASM__
      PlatformEnlightenmentProvider.Current.EnableWasm();
//#endif
```
