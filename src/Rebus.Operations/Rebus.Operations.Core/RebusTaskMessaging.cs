using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Bus;
using Rebus.Transport;

namespace Dbosoft.Rebus.Operations;

public class RebusTaskMessaging : ITaskMessaging
{
    private readonly IBus _bus;
    private readonly WorkflowOptions _options;
    private readonly ITaskCancellationRegistry _cancellationRegistry;

    public RebusTaskMessaging(IBus bus,
        WorkflowOptions options,
        ITaskCancellationRegistry cancellationRegistry)
    {
        _bus = bus;
        _options = options;
        _cancellationRegistry = cancellationRegistry;
    }

    public Task FailTask(IOperationTaskMessage message, string errorMessage, IDictionary<string,string>? additionalHeaders = null)
    {
        return FailTask(message, new ErrorData { ErrorMessage = errorMessage }, additionalHeaders);
    }

    // All terminal paths report the status event first and only drop the cancellation
    // registration after the send succeeds. If the send fails the message is retried with
    // the registration still in place, so the retried handler observes the existing (possibly
    // already cancelled) token instead of registering a fresh, non-cancelled one.
    public async Task FailTask(IOperationTaskMessage message, ErrorData error, IDictionary<string,string>? additionalHeaders = null)
    {
        await _bus.SendWorkflowEvent(_options,
            OperationTaskStatusEvent.Failed(
                message.OperationId, message.InitiatingTaskId,
                message.TaskId, error, _options.JsonSerializerOptions), additionalHeaders)
            .ConfigureAwait(false);
        _cancellationRegistry.Remove(message.OperationId, message.TaskId);
    }


    public async Task CompleteTask(IOperationTaskMessage message, IDictionary<string,string>? additionalHeaders = null)
    {
        await _bus.SendWorkflowEvent(_options,
            OperationTaskStatusEvent.Completed(
                message.OperationId, message.InitiatingTaskId, message.TaskId), additionalHeaders)
            .ConfigureAwait(false);
        _cancellationRegistry.Remove(message.OperationId, message.TaskId);
    }

    public async Task CompleteTask(IOperationTaskMessage message, object responseMessage, IDictionary<string,string>? additionalHeaders = null)
    {
        await _bus.SendWorkflowEvent(_options,
            OperationTaskStatusEvent.Completed(
                message.OperationId, message.InitiatingTaskId, message.TaskId, responseMessage,
                _options.JsonSerializerOptions), additionalHeaders)
            .ConfigureAwait(false);
        _cancellationRegistry.Remove(message.OperationId, message.TaskId);
    }

    public async Task CancelTask(IOperationTaskMessage message, IDictionary<string,string>? additionalHeaders = null)
    {
        await _bus.SendWorkflowEvent(_options,
            OperationTaskStatusEvent.Cancelled(
                message.OperationId, message.InitiatingTaskId, message.TaskId), additionalHeaders)
            .ConfigureAwait(false);
        _cancellationRegistry.Remove(message.OperationId, message.TaskId);
    }

    public CancellationToken GetCancellationToken(IOperationTaskMessage message)
    {
        return _cancellationRegistry.Register(message.OperationId, message.TaskId);
    }


    public Task ProgressMessage(IOperationTaskMessage message, object data, IDictionary<string,string>? additionalHeaders = null)
    {
        return ProgressMessage(message.OperationId, message.TaskId, data, additionalHeaders);
    }

    public async Task ProgressMessage(Guid operationId, Guid taskId, object data, IDictionary<string,string>? additionalHeaders = null)
    {
        using var scope = new RebusTransactionScope();
        await _bus.SendWorkflowEvent(_options, new OperationTaskProgressEvent
        {
            Id = Guid.NewGuid(),
            OperationId = operationId,
            TaskId = taskId,
            Data = data,
            Timestamp = DateTimeOffset.UtcNow
        }, additionalHeaders).ConfigureAwait(false);

        // commit it like this
        await scope.CompleteAsync().ConfigureAwait(false);
    }
}