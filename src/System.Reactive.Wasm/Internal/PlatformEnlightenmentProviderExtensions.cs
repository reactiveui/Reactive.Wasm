// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace System.Reactive.PlatformServices;

/// <summary>
/// Contains extension methods associated with registrations of platform enlightenment providers.
/// </summary>
public static class PlatformEnlightenmentProviderExtensions
{
    /// <summary>
    /// Sets the <see cref="PlatformEnlightenmentProvider.Current"/> to the <see cref="WasmPlatformEnlightenmentProvider"/> one.
    /// </summary>
    /// <param name="provider">The provider. This parameter is ignored.</param>
#pragma warning disable IDE0060
    public static void EnableWasm(this IPlatformEnlightenmentProvider provider)
#pragma warning restore IDE0060 // Remove unused parameter
    {
#pragma warning disable CS0618 // Type or member is obsolete
        PlatformEnlightenmentProvider.Current = new WasmPlatformEnlightenmentProvider();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
