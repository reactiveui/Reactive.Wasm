// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#pragma warning disable SA1402 // File may only contain a single type

namespace System.Reactive;

/// <summary>
/// Provides generic stub implementations for common functional patterns.
/// </summary>
/// <typeparam name="T">The type parameter for the stub implementations.</typeparam>
internal static class Stubs<T>
{
    /// <summary>
    /// Gets an action that ignores its input parameter.
    /// </summary>
    public static readonly Action<T> Ignore = _ => { };

    /// <summary>
    /// Gets a function that returns its input parameter unchanged (identity function).
    /// </summary>
    public static readonly Func<T, T> I = _ => _;
}

/// <summary>
/// Provides non-generic stub implementations for common functional patterns.
/// </summary>
internal static class Stubs
{
    /// <summary>
    /// Gets an action that performs no operation.
    /// </summary>
    public static readonly Action Nop = () => { };
}
