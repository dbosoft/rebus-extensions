using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Dbosoft.Rebus.Operations;

[PublicAPI]
public interface ITaskMessaging
{
    Task FailTask(IOperationTaskMessage message, string errorMessage,
        IDictionary<string, string>? additionalHeaders = null);

    Task FailTask(IOperationTaskMessage message, ErrorData error,
        IDictionary<string, string>? additionalHeaders = null);

    Task CompleteTask(IOperationTaskMessage message, IDictionary<string, string>? additionalHeaders = null);

    Task CompleteTask(IOperationTaskMessage message, object responseMessage,
        IDictionary<string, string>? additionalHeaders = null);

    /// <summary>
    /// Reports that the task was cancelled. Used by handlers (or the
    /// cancellation pipeline step) to end a task in the
    /// <see cref="OperationTaskStatus.Cancelled"/> state.
    /// </summary>
    Task CancelTask(IOperationTaskMessage message,
        IDictionary<string, string>? additionalHeaders = null);

    /// <summary>
    /// Opts the current task in to cooperative cancellation and returns a
    /// <see cref="CancellationToken"/> that is signalled when cancellation of
    /// the task's operation is requested. A handler threads the token into its
    /// long-running work; handlers that never call this are not cancellable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For a thrown <see cref="System.OperationCanceledException"/> to be reported as a
    /// cancelled task (instead of failing/retrying the message), the worker's Rebus
    /// pipeline must have the cancellation step installed via
    /// <c>OptionsConfigurer.EnableOperationCancellation</c>. Without it, a handler can
    /// still observe the token and call <see cref="CancelTask"/> itself.
    /// </para>
    /// <para>
    /// A cancellable handler stays in flight (holding a message-processing slot) until it
    /// is cancelled, so the worker host must be able to process the cancellation broadcast
    /// concurrently — configure it with more than one concurrent worker / parallelism.
    /// </para>
    /// </remarks>
    CancellationToken GetCancellationToken(IOperationTaskMessage message);

    Task ProgressMessage(IOperationTaskMessage message, object data,
        IDictionary<string, string>? additionalHeaders = null);

    Task ProgressMessage(Guid operationId, Guid taskId, object data,
        IDictionary<string, string>? additionalHeaders = null);
}