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
        private static bool? _isWasm;

        private static bool IsWasm
        {
            get
            {
                if (!_isWasm.HasValue)
                {
                    try
                    {
                        new Thread(() => { }).Start();
                        _isWasm = false;
                    }
                    catch (NotSupportedException)
                    {
                        _isWasm = true;
                    }
                }

                return _isWasm.Value;
            }
        }

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
