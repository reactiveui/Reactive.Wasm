// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER

using System.Reactive.Concurrency;
using Splat;

namespace ReactiveUI.Blazor;

/// <summary>
/// Extension methods for <see cref="IMutableDependencyResolver"/> that provide platform-specific
/// registration for Blazor WebAssembly.
/// </summary>
public static class ReactiveUIBuilderWasmExtensions
{
    /// <summary>
    /// Registers the <see cref="BlazorWasmScheduler"/> as the main thread scheduler
    /// for the application. This is the recommended setup for Blazor WASM on .NET 9+.
    /// </summary>
    /// <param name="resolver">The dependency resolver to configure.</param>
    /// <returns>The configured dependency resolver.</returns>
    public static IMutableDependencyResolver UseReactiveWasm(this IMutableDependencyResolver resolver)
    {
        // Explicitly register our new scheduler. This avoids reflection and
        // ensures the dependency is visible to the IL trimmer.
        resolver.RegisterConstant(new BlazorWasmScheduler(), typeof(IScheduler));
        return resolver;
    }
}

#endif