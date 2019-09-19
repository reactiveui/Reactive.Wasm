// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// WARNING: The full namespace-qualified type name should stay the same for the discovery in System.Reactive.Core to work!
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Threading;
using Splat;

namespace System.Reactive.PlatformServices
{
    /// <summary>
    /// (Infrastructure) Provider for platform-specific framework enlightenment.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WasmPlatformEnlightenmentProvider : CurrentPlatformEnlightenmentProvider
    {
        private static Lazy<bool> _isWasm = new Lazy<bool>(
            () =>
            {
                if (ModeDetector.InUnitTestRunner())
                {
                    return true;
                }

                try
                {
                    new Thread(() => { }).Start();
                    return false;
                }
                catch (Exception)
                {
                    // Usually a TypeInitializationException, however be safe by considering any platform
                    // that does not support threading as "Wasm".
                    return true;
                }
            }, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>Gets a value indicating whether the current executable is processing under WASM.</summary>
        public static bool IsWasm => _isWasm.Value;

        /// <summary>
        /// (Infastructure) Tries to gets the specified service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="args">Optional set of arguments.</param>
        /// <returns>Service instance or <c>null</c> if not found.</returns>
        public override T GetService<T>(object[] args)
        {
            if (IsWasm)
            {
                Type t = typeof(T);

                if (t == typeof(IConcurrencyAbstractionLayer))
                {
                    return (T)(object)new ConcurrencyAbstractionLayerWasmImpl();
                }
                else if (t == typeof(IScheduler))
                {
                    return (T)(object)WasmScheduler.Default;
                }
            }

            return base.GetService<T>(args);
        }
    }
}
