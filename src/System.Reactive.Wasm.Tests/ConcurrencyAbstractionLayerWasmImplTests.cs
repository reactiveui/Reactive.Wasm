// Copyright (c) 2019-2024 .NET Foundation and Contributors. All rights reserved.
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
        public void SetUp()
        {
            _concurrencyLayer = new ConcurrencyAbstractionLayerWasmImpl();
        }

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
    }
}
