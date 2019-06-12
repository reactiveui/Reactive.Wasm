using System;
using System.Collections.Generic;
using System.Text;

namespace System.Reactive.PlatformServices
{
    public static class PlatformEnlightenmentProviderExtensions
    {
        /// <summary>
        /// Sets the <see cref="PlatformEnlightenmentProvider.Current"/> to the <see cref="WasmPlatformEnlightenmentProvider"/> one.
        /// </summary>
#pragma warning disable IDE0060
        public static void EnableWasm(this IPlatformEnlightenmentProvider provider)
#pragma warning restore IDE0060 // Remove unused parameter
        {
#pragma warning disable CS0618 // Type or member is obsolete
            PlatformEnlightenmentProvider.Current = new WasmPlatformEnlightenmentProvider();
#pragma warning restore CS0618 // Type or member is obsolete
        }


    }
}
