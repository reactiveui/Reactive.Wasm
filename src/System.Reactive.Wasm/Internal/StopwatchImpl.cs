﻿// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.Reactive.Concurrency
{
    /// <summary>
    /// A stop watch implementation.
    /// </summary>
    internal class StopwatchImpl : IStopwatch
    {
        private readonly Stopwatch _sw;

        public StopwatchImpl()
        {
            _sw = Stopwatch.StartNew();
        }

        public TimeSpan Elapsed => _sw.Elapsed;
    }
}
