using System;
using System.Threading;
using JetBrains.Annotations;

namespace Dbosoft.Rebus.Operations;

/// <summary>
/// Process-local registry that bridges the worker's cancellation-event handler
/// and its running task handlers. A running task registers its
/// <see cref="CancellationTokenSource"/> here; when cancellation of the
/// operation is requested, <see cref="Cancel"/> trips the matching tokens so the
/// running handlers observe cancellation. The registry only signals the tokens —
/// the resulting status change is reported by the cancelled task over the normal
/// status-event channel.
/// </summary>
[PublicAPI]
public interface ITaskCancellationRegistry
{
    /// <summary>
    /// Registers the task for cancellation and returns a token that is signalled
    /// when <see cref="Cancel"/> is called for the task's operation. Idempotent:
    /// repeated calls for the same task return the same token.
    /// </summary>
    CancellationToken Register(Guid operationId, Guid taskId);

    /// <summary>
    /// Signals the cancellation tokens of all locally-registered tasks of the
    /// given operation. A no-op if no task of the operation is registered here.
    /// </summary>
    void Cancel(Guid operationId);

    /// <summary>
    /// Returns whether the given task is still registered and its token was tripped
    /// by <see cref="Cancel"/>. Used to distinguish a cancellation that this registry
    /// requested from an unrelated <see cref="System.OperationCanceledException"/>.
    /// </summary>
    bool IsCancellationRequested(Guid operationId, Guid taskId);

    /// <summary>
    /// Removes and disposes the registration for a finished task. Called on every
    /// terminal path (complete/fail/cancel).
    /// </summary>
    void Remove(Guid operationId, Guid taskId);
}
