using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations;

[PublicAPI]
public interface IOperationDispatcher
{
    ValueTask<IOperation> StartNew<TCommand>(
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null)
        where TCommand : class, new();

    ValueTask<IOperation> StartNew(
        Type commandType,
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null);

    ValueTask<IOperation> StartNew(
        object command,
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null);

    /// <summary>
    /// Requests cancellation of a running operation. Best-effort: only tasks
    /// whose handlers opted in to cancellation are interrupted. Cancelling a
    /// finished or unknown operation is a no-op.
    /// </summary>
    /// <remarks>
    /// The request is a broadcast on the workflow-event channel, so it reaches every
    /// worker host only when events are dispatched pub/sub
    /// (<see cref="WorkflowEventDispatchMode.Publish"/> with all task-running hosts
    /// subscribed). In <see cref="WorkflowEventDispatchMode.Send"/> mode the request is
    /// point-to-point and only reaches the single events destination, so it is reliable
    /// only when all task processing is co-located there.
    /// </remarks>
    ValueTask RequestCancellation(
        Guid operationId,
        IDictionary<string,string>? additionalHeaders = null);
}
