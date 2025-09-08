// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace System.Reactive;

/// <summary>
/// Provides timer stub implementations for reactive operations.
/// </summary>
internal static class TimerStubs
{
    /// <summary>
    /// Gets a timer that never fires, used as a placeholder for disposed timers.
    /// </summary>
    public static readonly Timer Never = new (_ => { });
}
