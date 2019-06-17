Enables Rx extensions in web-assembly projects.
Based on [Uno.Rx.NET](https://github.com/nventive/Uno.Rx.NET).

This must be manually enabled, in the entry point of your project, call the following (uncomment in multi-platform apps and change directive if necessary):

```c#
//#if __WASM__
      PlatformEnlightenmentProvider.Current.EnableWasm();
//#endif
```
