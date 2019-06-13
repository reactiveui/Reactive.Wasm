// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// WARNING: The full namespace-qualified type name should stay the same for the discovery in System.Reactive.Core to work!
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Reactive.PlatformServices
{
    /// <summary>
    /// (Infrastructure) Provider for platform-specific framework enlightenment.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WasmPlatformEnlightenmentProvider : CurrentPlatformEnlightenmentProvider
    {
        private static readonly bool _isWasm = RuntimeInformation.IsOSPlatform(OSPlatform.Create("WEBASSEMBLY"));

        /// <summary>
        /// (Infastructure) Tries to gets the specified service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="args">Optional set of arguments.</param>
        /// <returns>Service instance or <c>null</c> if not found.</returns>
        public override T GetService<T>(object[] args)
        {
            var t = typeof(T);

            bool allowsThreads = true;

            try
            {
                new Thread(() => { }).Start();
            }
            catch (NotSupportedException)
            {
                allowsThreads = false;
            }

            if (!allowsThreads && t == typeof(IConcurrencyAbstractionLayer))
            {
                return (T)(object)new ConcurrencyAbstractionLayerWasmImpl();
            }

            if (t == typeof(IScheduler) && args != null)
            {
                return (T)(object)WasmScheduler.Default;
            }

            return base.GetService<T>(args);
        }
    }
}
