// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Concurrency;

using Splat;

namespace System.Reactive.PlatformServices;

/// <summary>
/// (Infrastructure) Provider for platform-specific framework enlightenment.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class WasmPlatformEnlightenmentProvider : CurrentPlatformEnlightenmentProvider
{
    private static Lazy<bool> _isWasm = new (
        () => ModeDetector.InUnitTestRunner() || MonoTest, LazyThreadSafetyMode.PublicationOnly);

    /// <summary>Gets a value indicating whether the current executable is processing under WASM.</summary>
    public static bool IsWasm => _isWasm.Value;

    /// <summary> Gets a value indicating whether we're running on mono, hence wasm. </summary>
    private static bool MonoTest =>
        Type.GetType("Mono.Runtime") != null;

    /// <summary>
    /// (Infrastructure) Tries to gets the specified service.
    /// </summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <param name="args">Optional set of arguments.</param>
    /// <returns>Service instance or <c>null</c> if not found.</returns>
    public override T? GetService<T>(object[] args)
        where T : class
    {
        if (!IsWasm)
        {
            return base.GetService<T>(args);
        }

        var t = typeof(T);

        if (t == typeof(IConcurrencyAbstractionLayer))
        {
            return (T)(object)new ConcurrencyAbstractionLayerWasmImpl();
        }

        if (t == typeof(IScheduler))
        {
            return (T)(object)WasmScheduler.Default;
        }

        return base.GetService<T>(args);
    }
}
