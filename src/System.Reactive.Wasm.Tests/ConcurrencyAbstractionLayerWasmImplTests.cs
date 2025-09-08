// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;
using NUnit.Framework;

namespace System.Reactive.Wasm.Tests
{
    /// <summary>
    /// Tests for ConcurrencyAbstractionLayerWasmImpl.
    /// </summary>
    [TestFixture]
    public class ConcurrencyAbstractionLayerWasmImplTests
    {
        private ConcurrencyAbstractionLayerWasmImpl _concurrencyLayer = null!;

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public void SetUp() => _concurrencyLayer = new ConcurrencyAbstractionLayerWasmImpl();

        /// <summary>
        /// Tests that StartStopwatch returns a running stopwatch.
        /// </summary>
        [Test]
        public void StartStopwatch_ShouldReturnRunningStopwatch()
        {
            // Act
            var stopwatch = _concurrencyLayer.StartStopwatch();

            // Assert
            Assert.That(stopwatch, Is.Not.Null);
            Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(TimeSpan.Zero));

            // Wait a brief moment and check that elapsed time has increased
            Thread.Sleep(1);
            var elapsed1 = stopwatch.Elapsed;
            Thread.Sleep(1);
            var elapsed2 = stopwatch.Elapsed;

            Assert.That(elapsed2, Is.GreaterThan(elapsed1));
        }

        /// <summary>
        /// Tests that QueueUserWorkItem executes the provided action.
        /// </summary>
        [Test]
        public void QueueUserWorkItem_ShouldExecuteAction()
        {
            // Arrange
            var executed = false;
            var testState = "test_state";
            object? receivedState = null;

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            _concurrencyLayer.QueueUserWorkItem(
                state =>
                {
                    executed = true;
                    receivedState = state;
                    waitHandle.Set();
                }, testState);

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Action should have been executed within timeout");
            Assert.That(executed, Is.True, "Action should have been executed");
            Assert.That(receivedState, Is.EqualTo(testState), "Action should receive the correct state");
        }

        /// <summary>
        /// Tests that QueueUserWorkItem with null state executes correctly.
        /// </summary>
        [Test]
        public void QueueUserWorkItem_WithNullState_ShouldExecuteAction()
        {
            // Arrange
            var executed = false;
            object? receivedState = "not_null";

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            _concurrencyLayer.QueueUserWorkItem(
                state =>
                {
                    executed = true;
                    receivedState = state;
                    waitHandle.Set();
                }, null);

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Action should have been executed within timeout");
            Assert.That(executed, Is.True, "Action should have been executed");
            Assert.That(receivedState, Is.Null, "Action should receive null state");
        }

        /// <summary>
        /// Tests that SupportsLongRunning returns false for WASM implementation.
        /// </summary>
        [Test]
        public void SupportsLongRunning_ShouldReturnFalse() =>

            // Act & Assert
            Assert.That(_concurrencyLayer.SupportsLongRunning, Is.False);

        /// <summary>
        /// Tests that Sleep method handles positive timeouts correctly.
        /// </summary>
        [Test]
        public void Sleep_WithPositiveTimeout_ShouldComplete()
        {
            // Arrange
            var timeout = TimeSpan.FromMilliseconds(10);
            var stopwatch = Diagnostics.Stopwatch.StartNew();

            // Act
            _concurrencyLayer.Sleep(timeout);
            stopwatch.Stop();

            // Assert - should have slept at least the requested time
            Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(timeout));
        }

        /// <summary>
        /// Tests that Sleep method handles negative timeouts by treating them as zero.
        /// </summary>
        [Test]
        public void Sleep_WithNegativeTimeout_ShouldNotThrow()
        {
            // Arrange
            var negativeTimeout = TimeSpan.FromMilliseconds(-100);

            // Act & Assert - should not throw and should complete quickly
            Assert.DoesNotThrow(() => _concurrencyLayer.Sleep(negativeTimeout));
        }

        /// <summary>
        /// Tests that StartThread executes the provided action on a background thread.
        /// </summary>
        [Test]
        public void StartThread_ShouldExecuteActionOnBackgroundThread()
        {
            // Arrange
            var executed = false;
            var testState = "thread_test";
            object? receivedState = null;
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            var executionThreadId = 0;

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            _concurrencyLayer.StartThread(
                state =>
                {
                    executed = true;
                    receivedState = state;
                    executionThreadId = Thread.CurrentThread.ManagedThreadId;
                    waitHandle.Set();
                }, testState);

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Action should have been executed within timeout");
            Assert.That(executed, Is.True, "Action should have been executed");
            Assert.That(receivedState, Is.EqualTo(testState), "Action should receive the correct state");
            Assert.That(executionThreadId, Is.Not.EqualTo(currentThreadId), "Action should execute on a different thread");
        }

        /// <summary>
        /// Tests that StartTimer executes the action after the specified delay.
        /// </summary>
        [Test]
        public void StartTimer_ShouldExecuteAfterDelay()
        {
            // Arrange
            var executed = false;
            var testState = "timer_test";
            object? receivedState = null;
            var delay = TimeSpan.FromMilliseconds(50);

            using var waitHandle = new ManualResetEventSlim(false);
            var stopwatch = Diagnostics.Stopwatch.StartNew();

            // Act
            using var timer = _concurrencyLayer.StartTimer(
                state =>
                {
                    executed = true;
                    receivedState = state;
                    waitHandle.Set();
                },
                testState,
                delay);

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            stopwatch.Stop();

            Assert.That(completed, Is.True, "Timer action should have been executed within timeout");
            Assert.That(executed, Is.True, "Timer action should have been executed");
            Assert.That(receivedState, Is.EqualTo(testState), "Timer action should receive the correct state");
            Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(delay), "Timer should not execute before the delay");
        }

        /// <summary>
        /// Tests that StartTimer with negative delay executes immediately.
        /// </summary>
        [Test]
        public void StartTimer_WithNegativeDelay_ShouldExecuteImmediately()
        {
            // Arrange
            var executed = false;
            var negativeDelay = TimeSpan.FromMilliseconds(-100);

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            using var timer = _concurrencyLayer.StartTimer(
                _ =>
                {
                    executed = true;
                    waitHandle.Set();
                },
                null,
                negativeDelay);

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Timer action should have been executed within timeout");
            Assert.That(executed, Is.True, "Timer action should have been executed");
        }

        /// <summary>
        /// Tests that StartPeriodicTimer throws for negative periods.
        /// </summary>
        [Test]
        public void StartPeriodicTimer_WithNegativePeriod_ShouldThrow()
        {
            // Arrange
            var negativePeriod = TimeSpan.FromMilliseconds(-100);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _concurrencyLayer.StartPeriodicTimer(() => { }, negativePeriod));
        }

        /// <summary>
        /// Tests that StartPeriodicTimer with zero period creates a FastPeriodicTimer.
        /// </summary>
        [Test]
        public void StartPeriodicTimer_WithZeroPeriod_ShouldExecuteMultipleTimes()
        {
            // Arrange
            var executionCount = 0;
            var maxExecutions = 5;

            using var completionSource = new CancellationTokenSource();

            // Act
            using var periodicTimer = _concurrencyLayer.StartPeriodicTimer(
                () =>
                {
                    if (Interlocked.Increment(ref executionCount) >= maxExecutions)
                    {
                        completionSource.Cancel();
                    }
                },
                TimeSpan.Zero);

            // Assert - wait for multiple executions or timeout
            var completed = completionSource.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2));
            Assert.That(completed, Is.True, "Periodic timer should have executed multiple times");
            Assert.That(
                executionCount,
                Is.GreaterThanOrEqualTo(maxExecutions),
                $"Should have executed at least {maxExecutions} times, but executed {executionCount} times");
        }

        /// <summary>
        /// Tests that StartPeriodicTimer with positive period executes periodically.
        /// </summary>
        [Test]
        public void StartPeriodicTimer_WithPositivePeriod_ShouldExecuteMultipleTimes()
        {
            // Arrange
            var executionCount = 0;
            var period = TimeSpan.FromMilliseconds(50);
            var expectedExecutions = 3;

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            using var periodicTimer = _concurrencyLayer.StartPeriodicTimer(
                () =>
                {
                    if (Interlocked.Increment(ref executionCount) >= expectedExecutions)
                    {
                        waitHandle.Set();
                    }
                },
                period);

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Periodic timer should have executed expected number of times");
            Assert.That(
                executionCount,
                Is.GreaterThanOrEqualTo(expectedExecutions),
                $"Should have executed at least {expectedExecutions} times, but executed {executionCount} times");
        }
    }
}
