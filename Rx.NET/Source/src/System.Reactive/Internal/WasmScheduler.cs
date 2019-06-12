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
    class WasmScheduler : LocalScheduler, ISchedulerPeriodic
    {
        private static Lazy<WasmScheduler> s_default = new Lazy<WasmScheduler>(() => new WasmScheduler());

        // Import from https://github.com/mono/mono/blob/0a8126c2094d2d0800a462d4d0c790d4db421477/mcs/class/corlib/System.Threading/Timer.cs#L39
        internal static class WasmRuntime
        {
            static Dictionary<int, Action> callbacks;
            static int next_id;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            static extern void SetTimeout(int timeout, int id);

            internal static void ScheduleTimeout(int timeout, Action action)
            {
                if (callbacks == null)
                {
                    callbacks = new Dictionary<int, Action>();
                }

                int id = ++next_id;
                callbacks[id] = action;
                SetTimeout(timeout, id);
            }

            //XXX Keep this in sync with mini-wasm.c:mono_set_timeout_exec
            static void TimeoutCallback(int id)
            {
                var cb = callbacks[id];
                callbacks.Remove(id);
                cb();
            }
        }

        /// <summary>
        /// Constructs a WasmScheduler that schedules units of work on the Windows ThreadPool.
        /// </summary>
        public WasmScheduler()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the Windows Runtime thread pool scheduler.
        /// </summary>
        public static WasmScheduler Default
        {
            get
            {
                return s_default.Value;
            }
        }

        /// <summary>
        /// Schedules an action to be executed.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">State passed to the action to be executed.</param>
        /// <param name="action">Action to be executed.</param>
        /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
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
            //
            // The WinRT thread pool is based on the Win32 thread pool and cannot handle
            // sub-1ms resolution. When passing a lower period, we get single-shot
            // timer behavior instead. See MSDN documentation for CreatePeriodicTimer
            // for more information.
            //
            if (period < TimeSpan.FromMilliseconds(1))
            {
                throw new ArgumentOutOfRangeException("period", "The WinRT thread pool doesn't support creating periodic timers with a period below 1 millisecond.");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
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
                  run
                );
                  });
                };
              }
            );

            return Disposable.Create(() =>
            {
                gate.Dispose();
                action = Stubs<TState>.I;
            });
        }

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
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
                }
            );

            return d;
        }
    }
}
#endif