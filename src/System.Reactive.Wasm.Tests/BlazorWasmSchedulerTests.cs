// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ReactiveUI.Blazor;

namespace System.Reactive.Wasm.Tests;

/// <summary>
/// Tests for BlazorWasmScheduler.
/// Note: These tests validate basic functionality but cannot fully test the Blazor-specific
/// SynchronizationContext behavior without running in a Blazor environment.
/// </summary>
[TestFixture]
public class BlazorWasmSchedulerTests
{
    /// <summary>
    /// Tests that BlazorWasmScheduler can be instantiated when a SynchronizationContext is available.
    /// </summary>
    [Test]
    public void Constructor_WithSynchronizationContext_ShouldSucceed()
    {
        // Arrange: Set up a SynchronizationContext (simulating Blazor environment)
        var originalContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        try
        {
            // Act & Assert: Should not throw
            var scheduler = new BlazorWasmScheduler();
            Assert.That(scheduler, Is.Not.Null);
            Assert.That(scheduler.Now, Is.Not.EqualTo(DateTimeOffset.MinValue));
        }
        finally
        {
            // Cleanup: Restore original context
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }

    /// <summary>
    /// Tests that BlazorWasmScheduler throws when no SynchronizationContext is available.
    /// </summary>
    [Test]
    public void Constructor_WithoutSynchronizationContext_ShouldThrow()
    {
        // Arrange: Ensure no SynchronizationContext is set
        var originalContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new BlazorWasmScheduler());
        }
        finally
        {
            // Cleanup: Restore original context
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }

    /// <summary>
    /// Tests basic scheduler functionality with SynchronizationContext.
    /// </summary>
    [Test]
    public void Schedule_BasicAction_ShouldExecute()
    {
        // Arrange
        var originalContext = SynchronizationContext.Current;
        var testContext = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(testContext);

        try
        {
            var scheduler = new BlazorWasmScheduler();
            bool executed = false;
            string? receivedState = null;

            // Act
            var disposable = scheduler.Schedule("test", (s, state) =>
            {
                executed = true;
                receivedState = state;
                return System.Reactive.Disposables.Disposable.Empty;
            });

            // Process queued operations in test context
            testContext.ProcessQueue();

            // Assert
            Assert.That(executed, Is.True, "Action should have been executed");
            Assert.That(receivedState, Is.EqualTo("test"), "Action should receive the correct state");
            Assert.That(disposable, Is.Not.Null, "Schedule should return a disposable");
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }

    /// <summary>
    /// A test SynchronizationContext that allows us to control when posted actions execute.
    /// </summary>
    private class TestSynchronizationContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback callback, object? state)> _queue = new ();

        public override void Post(SendOrPostCallback d, object? state)
        {
            _queue.Enqueue((d, state));
        }

        public void ProcessQueue()
        {
            while (_queue.Count > 0)
            {
                var (callback, state) = _queue.Dequeue();
                callback(state);
            }
        }
    }
}

#endif