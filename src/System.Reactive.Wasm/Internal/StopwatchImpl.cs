// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.Reactive.Concurrency;

/// <summary>
/// A stop watch implementation.
/// </summary>
internal class StopwatchImpl : IStopwatch
{
    private readonly Stopwatch _stopWatch;

    /// <summary>
    /// Initializes a new instance of the <see cref="StopwatchImpl"/> class.
    /// </summary>
    public StopwatchImpl() => _stopWatch = Stopwatch.StartNew();

    /// <summary>
    /// Gets the elapsed time since the stopwatch was started.
    /// </summary>
    public TimeSpan Elapsed => _stopWatch.Elapsed;
}
