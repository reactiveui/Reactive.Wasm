// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Concurrency
{
    /// <summary>
    /// A scheduler for the WASM systems.
    /// </summary>
    public class WasmScheduler : LocalScheduler, ISchedulerPeriodic
    {
        private static readonly Lazy<WasmScheduler> _default = new Lazy<WasmScheduler>(() => new WasmScheduler());

        /// <summary>
        /// Gets the singleton instance of the Windows Runtime thread pool scheduler.
        /// </summary>
        public static WasmScheduler Default => _default.Value;

        /// <inheritdoc />
        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var d = new SingleAssignmentDisposable();

            WasmRuntime.ScheduleTimeout(0, () =>
            {
                if (!d.IsDisposed)
                {
                    d.Disposable = action(this, state);
                }
            });

            return d;
        }

        /// <summary>
        /// Schedules a periodic piece of work, using a Windows.System.Threading.ThreadPoolTimer object.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">Initial state passed to the action upon the first iteration.</param>
        /// <param name="period">Period for running the work periodically.</param>
        /// <param name="action">Action to be executed, potentially updating the state.</param>
        /// <returns>The disposable object used to cancel the scheduled recurring action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is less than one millisecond.</exception>
        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            // The WinRT thread pool is based on the Win32 thread pool and cannot handle
            // sub-1ms resolution. When passing a lower period, we get single-shot
            // timer behavior instead. See MSDN documentation for CreatePeriodicTimer
            // for more information.
            if (period < TimeSpan.FromMilliseconds(1))
            {
                throw new ArgumentOutOfRangeException(nameof(period), "The WinRT thread pool doesn't support creating periodic timers with a period below 1 millisecond.");
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var state1 = state;
            var gate = new AsyncLock();

            WasmRuntime.ScheduleTimeout(
              (int)period.TotalMilliseconds,
              () =>
              {
                  Action run = null;

                  run = () =>
                  {
                      gate.Wait(() =>
                      {
                          state1 = action(state1);

                          WasmRuntime.ScheduleTimeout(
                          (int)period.TotalMilliseconds,
                          run);
                      });
                  };
              });

            return Disposable.Create(() =>
            {
                gate.Dispose();
                action = Stubs<TState>.I;
            });
        }

        /// <inheritdoc />
        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var dt = Scheduler.Normalize(dueTime);

            if (dt.Ticks == 0)
            {
                return Schedule(state, action);
            }

            var d = new SingleAssignmentDisposable();

            WasmRuntime.ScheduleTimeout(
                (int)dt.TotalMilliseconds,
                () =>
                {
                    if (!d.IsDisposed)
                    {
                        d.Disposable = action(this, state);
                    }
                });

            return d;
        }

        // Import from https://github.com/mono/mono/blob/0a8126c2094d2d0800a462d4d0c790d4db421477/mcs/class/corlib/System.Threading/Timer.cs#L39
        internal static class WasmRuntime
        {
            private static Dictionary<int, Action> _callbacks;
            private static int _next_id;

            internal static void ScheduleTimeout(int timeout, Action action)
            {
                if (_callbacks == null)
                {
                    _callbacks = new Dictionary<int, Action>();
                }

                int id = ++_next_id;
                _callbacks[id] = action;
                SetTimeout(timeout, id);
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private static extern void SetTimeout(int timeout, int id);

            // XXX Keep this in sync with mini-wasm.c:mono_set_timeout_exec
            private static void TimeoutCallback(int id)
            {
                var cb = _callbacks[id];
                _callbacks.Remove(id);
                cb();
            }
        }
    }
}
#endif
