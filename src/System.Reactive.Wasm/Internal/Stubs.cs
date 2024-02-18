// Copyright (c) 2019-2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace System.Reactive
{
#pragma warning disable SA1402 // File may only contain a single type
    internal static class Stubs<T>
#pragma warning restore SA1402 // File may only contain a single type
    {
        public static readonly Action<T> Ignore = _ => { };
        public static readonly Func<T, T> I = _ => _;
    }

    internal static class Stubs
    {
        public static readonly Action Nop = () => { };
    }
}
