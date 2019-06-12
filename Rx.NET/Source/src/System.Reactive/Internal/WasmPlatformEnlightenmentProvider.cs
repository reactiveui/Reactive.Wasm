// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

//
// WARNING: The full namespace-qualified type name should stay the same for the discovery in System.Reactive.Core to work!
//
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Reactive.PlatformServices
{
    /// <summary>
    /// (Infrastructure) Provider for platform-specific framework enlightenments.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WasmPlatformEnlightenmentProvider : CurrentPlatformEnlightenmentProvider
    {
        private readonly static bool _isWasm = RuntimeInformation.IsOSPlatform(OSPlatform.Create("WEBASSEMBLY"));

        /// <summary>
        /// (Infastructure) Tries to gets the specified service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="args">Optional set of arguments.</param>
        /// <returns>Service instance or <c>null</c> if not found.</returns>
        public override T GetService<T>(object[] args) //where T : class
        {
            var t = typeof(T);

#if !NO_THREAD || WINDOWS
            if (t == typeof(IConcurrencyAbstractionLayer))
            {
#if NETSTANDARD2_0
                if (_isWasm)
                {
                    return (T)(object)new ConcurrencyAbstractionLayerWasmImpl();
                }
#endif
            }
#endif

            if (t == typeof(IScheduler) && args != null)
            {
#if NETSTANDARD2_0
                if (_isWasm)
                {
                    return (T)(object)WasmScheduler.Default;
                }
#endif
            }

            return base.GetService<T>(args);
        }
    }
}