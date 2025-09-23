// Copyright (c) 2019-2025 ReactiveUI. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER

using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace ReactiveUI.Blazor;

/// <summary>
/// A scheduler that is compatible with the .NET 9 Blazor WebAssembly
/// "deputy thread" model. It correctly marshals actions to the UI thread
/// via the Blazor SynchronizationContext.
/// </summary>
public class BlazorWasmScheduler : IScheduler
{
    private readonly SynchronizationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlazorWasmScheduler"/> class.
    /// </summary>
    public BlazorWasmScheduler()
    {
        // In the .NET 9 deputy thread model, we must capture the SynchronizationContext
        // that is associated with the Blazor renderer's dispatcher. A reliable way
        // to do this is by instantiating a dummy component, which captures the
        // context during its initialization.
        var dummyComponent = new DummyComponent();
        _context = SynchronizationContext.Current ?? throw new InvalidOperationException("Could not capture the Blazor SynchronizationContext. Ensure the scheduler is initialized on the main thread.");
    }

    /// <inheritdoc/>
    public DateTimeOffset Now => DateTimeOffset.Now;

    /// <inheritdoc/>
    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        var disposable = new SingleAssignmentDisposable();

        // Use Post to asynchronously dispatch the action to the UI thread's work queue.
        // This is non-blocking and safe to call from the deputy thread.
        _context.Post(
            _ =>
            {
                if (!disposable.IsDisposed)
                {
                    disposable.Disposable = action(this, state);
                }
            },
            null);

        return disposable;
    }

    /// <inheritdoc/>
    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        var disposable = new MultipleAssignmentDisposable();

        var timer = new Timer(
            _ =>
            {
                if (!disposable.IsDisposed)
                {
                    disposable.Disposable = Schedule(state, action);
                }
            },
            null,
            dueTime,
            Timeout.InfiniteTimeSpan);

        // Ensure the timer is disposed when the scheduled action is unsubscribed.
        disposable.Disposable = new DisposableAction(() => timer.Dispose());
        return disposable;
    }

    /// <inheritdoc/>
    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) =>
        Schedule(state, dueTime - Now, action);

    // A private, lightweight component used solely to capture the SynchronizationContext.
    private sealed class DummyComponent : IComponent
    {
        public void Attach(RenderHandle renderHandle)
        {
            // No-op. We only need the constructor's side-effect.
        }

        public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
    }

    private sealed class DisposableAction : IDisposable
    {
        private readonly Action _action;

        public DisposableAction(Action action) => _action = action;

        public void Dispose() => _action?.Invoke();
    }
}

#endif