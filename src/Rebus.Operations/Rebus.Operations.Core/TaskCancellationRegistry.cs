using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Dbosoft.Rebus.Operations;

/// <summary>
/// Process-local, thread-safe implementation of <see cref="ITaskCancellationRegistry"/>.
/// Holds one <see cref="CancellationTokenSource"/> per running, cancellable task.
/// </summary>
/// <remarks>
/// <see cref="CancellationTokenSource"/> is <see cref="IDisposable"/>, so disposal is handled
/// carefully: each source is disposed exactly once by the thread that removes it, <see cref="Cancel"/>
/// only signals (never disposes) and tolerates a racing removal, and any sources left over when the
/// registry itself is disposed are cleaned up.
/// </remarks>
public sealed class TaskCancellationRegistry : ITaskCancellationRegistry, IDisposable
{
    private readonly ConcurrentDictionary<(Guid OperationId, Guid TaskId), CancellationTokenSource> _sources = new();
    private int _disposed;

    public CancellationToken Register(Guid operationId, Guid taskId)
    {
        var key = (operationId, taskId);

        // Idempotent: a second registration for the same task returns the existing token.
        if (_sources.TryGetValue(key, out var existing))
            return existing.Token;

        // GetOrAdd with a factory can build more than one source under contention; create
        // the source ourselves and dispose the one that loses the race so none leaks.
        var created = new CancellationTokenSource();
        var winner = _sources.GetOrAdd(key, created);
        if (!ReferenceEquals(winner, created))
            created.Dispose();
        return winner.Token;
    }

    public bool IsCancellationRequested(Guid operationId, Guid taskId)
    {
        // True only while the task is still registered (i.e. before its terminal
        // Remove) and its token was actually tripped by Cancel.
        return _sources.TryGetValue((operationId, taskId), out var source)
               && source.IsCancellationRequested;
    }

    public void Cancel(Guid operationId)
    {
        foreach (var entry in _sources)
        {
            if (entry.Key.OperationId != operationId)
                continue;

            try
            {
                entry.Value.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The task finished and removed/disposed its source concurrently; nothing to cancel.
            }
        }
    }

    public void Remove(Guid operationId, Guid taskId)
    {
        if (_sources.TryRemove((operationId, taskId), out var source))
            source.Dispose();
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            return;

        foreach (var key in _sources.Keys)
        {
            if (_sources.TryRemove(key, out var source))
                source.Dispose();
        }
    }
}
