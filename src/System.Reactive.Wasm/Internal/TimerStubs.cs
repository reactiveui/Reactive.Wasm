// Copyright (c) 2019-2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace System.Reactive
{
    internal static class TimerStubs
    {
        public static readonly System.Threading.Timer Never = new System.Threading.Timer(_ => { });
    }
}
