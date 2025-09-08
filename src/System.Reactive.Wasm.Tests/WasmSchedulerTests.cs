// Copyright (c) 2019-2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using NUnit.Framework;

namespace System.Reactive.Wasm.Tests
{
    /// <summary>
    /// Integration-style tests for WasmScheduler.
    /// Note: These tests cannot use traditional mocking as WasmScheduler relies on WasmRuntime,
    /// which is internal to the Mono runtime. Instead, we schedule simple actions and verify
    /// they execute correctly.
    /// </summary>
    [TestFixture]
    public class WasmSchedulerTests
    {
        private WasmScheduler _scheduler = null!;

        /// <summary>
        /// Sets up the test fixture.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _scheduler = WasmScheduler.Default;
        }

        /// <summary>
        /// Tests immediate scheduling - action should execute immediately.
        /// </summary>
        [Test]
        public void Schedule_ImmediateAction_ShouldExecute()
        {
            // Arrange
            var executed = false;
            var testState = "immediate_test";
            string? receivedState = null;

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            var disposable = _scheduler.Schedule(testState, (scheduler, state) =>
            {
                executed = true;
                receivedState = state;
                waitHandle.Set();
                return Disposable.Empty;
            });

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Action should have been executed within timeout");
            Assert.That(executed, Is.True, "Action should have been executed");
            Assert.That(receivedState, Is.EqualTo(testState), "Action should receive the correct state");
            Assert.That(disposable, Is.Not.Null, "Schedule should return a disposable");
        }

        /// <summary>
        /// Tests delayed scheduling - action should execute after the specified delay.
        /// </summary>
        [Test]
        public void Schedule_DelayedAction_ShouldExecuteAfterDelay()
        {
            // Arrange
            var executed = false;
            var testState = "delayed_test";
            string? receivedState = null;
            var delay = TimeSpan.FromMilliseconds(100);

            using var waitHandle = new ManualResetEventSlim(false);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var disposable = _scheduler.Schedule(testState, delay, (scheduler, state) =>
            {
                executed = true;
                receivedState = state;
                waitHandle.Set();
                return Disposable.Empty;
            });

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            stopwatch.Stop();

            Assert.That(completed, Is.True, "Action should have been executed within timeout");
            Assert.That(executed, Is.True, "Action should have been executed");
            Assert.That(receivedState, Is.EqualTo(testState), "Action should receive the correct state");
            Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(delay), "Action should not execute before the delay");
            Assert.That(disposable, Is.Not.Null, "Schedule should return a disposable");
        }

        /// <summary>
        /// Tests that zero delay scheduling executes immediately.
        /// </summary>
        [Test]
        public void Schedule_ZeroDelay_ShouldExecuteImmediately()
        {
            // Arrange
            var executed = false;
            var testState = "zero_delay_test";

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            var disposable = _scheduler.Schedule(testState, TimeSpan.Zero, (scheduler, state) =>
            {
                executed = true;
                waitHandle.Set();
                return Disposable.Empty;
            });

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            Assert.That(completed, Is.True, "Action should have been executed within timeout");
            Assert.That(executed, Is.True, "Action should have been executed");
            Assert.That(disposable, Is.Not.Null, "Schedule should return a disposable");
        }

        /// <summary>
        /// Tests periodic scheduling - action should execute multiple times at regular intervals.
        /// </summary>
        [Test]
        public void SchedulePeriodic_ShouldExecuteMultipleTimes()
        {
            // Arrange
            var executionCount = 0;
            var initialState = 0;
            var period = TimeSpan.FromMilliseconds(50);
            var expectedExecutions = 3;

            using var waitHandle = new ManualResetEventSlim(false);

            // Act
            var disposable = _scheduler.SchedulePeriodic(initialState, period, state =>
            {
                var newState = state + 1;
                Interlocked.Increment(ref executionCount);

                if (newState >= expectedExecutions)
                {
                    waitHandle.Set();
                }

                return newState;
            });

            // Assert
            var completed = waitHandle.Wait(TimeSpan.FromSeconds(5));
            disposable.Dispose();

            Assert.That(completed, Is.True, "Periodic action should have executed expected number of times");
            Assert.That(
                executionCount,
                Is.GreaterThanOrEqualTo(expectedExecutions),
                $"Should have executed at least {expectedExecutions} times, but executed {executionCount} times");
        }

        /// <summary>
        /// Tests that periodic scheduling throws for periods less than 1 millisecond.
        /// </summary>
        [Test]
        public void SchedulePeriodic_WithSubMillisecondPeriod_ShouldThrow()
        {
            // Arrange
            var subMillisecondPeriod = TimeSpan.FromTicks(5000); // 0.5 milliseconds

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _scheduler.SchedulePeriodic(0, subMillisecondPeriod, state => state));
        }

        /// <summary>
        /// Tests that scheduling with null action throws.
        /// </summary>
        [Test]
        public void Schedule_WithNullAction_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _scheduler.Schedule("test", (Func<IScheduler, string, IDisposable>)null!));
        }

        /// <summary>
        /// Tests that delayed scheduling with null action throws.
        /// </summary>
        [Test]
        public void Schedule_DelayedWithNullAction_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _scheduler.Schedule("test", TimeSpan.FromMilliseconds(100), (Func<IScheduler, string, IDisposable>)null!));
        }

        /// <summary>
        /// Tests that periodic scheduling with null action throws.
        /// </summary>
        [Test]
        public void SchedulePeriodic_WithNullAction_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _scheduler.SchedulePeriodic(0, TimeSpan.FromMilliseconds(100), (Func<int, int>)null!));
        }
    }
}