// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace System.Reactive
{
    internal static class TimerStubs
    {
#if NETSTANDARD1_3
        public static readonly System.Threading.Timer Never = new System.Threading.Timer(_ => { }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
#else
        public static readonly System.Threading.Timer Never = new System.Threading.Timer(_ => { });
#endif
    }
}
