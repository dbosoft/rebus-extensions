using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Bus;
using Rebus.Transport;

namespace Dbosoft.Rebus.Operations;

public class RebusTaskMessaging : ITaskMessaging
{
    private readonly IBus _bus;
    private readonly WorkflowOptions _options;

    public RebusTaskMessaging(IBus bus, 
        WorkflowOptions options)
    {
        _bus = bus;
        _options = options;
    }
    
    public Task FailTask(IOperationTaskMessage message, string errorMessage, IDictionary<string,string>? additionalHeaders = null)
    {
        return FailTask(message, new ErrorData { ErrorMessage = errorMessage }, additionalHeaders);
    }

    public Task FailTask(IOperationTaskMessage message, ErrorData error, IDictionary<string,string>? additionalHeaders = null)
    {
        return _bus.SendWorkflowEvent(_options,
            OperationTaskStatusEvent.Failed(
                message.OperationId, message.InitiatingTaskId,
                message.TaskId, error,_options.JsonSerializerOptions),additionalHeaders );
    }


    public Task CompleteTask(IOperationTaskMessage message, IDictionary<string,string>? additionalHeaders = null)
    {
        return _bus.SendWorkflowEvent(_options,
            OperationTaskStatusEvent.Completed(
                message.OperationId, message.InitiatingTaskId, message.TaskId), additionalHeaders);
    }

    public Task CompleteTask(IOperationTaskMessage message, object responseMessage, IDictionary<string,string>? additionalHeaders = null)
    {
        return _bus.SendWorkflowEvent(_options,
            OperationTaskStatusEvent.Completed(
                message.OperationId, message.InitiatingTaskId, message.TaskId, responseMessage, 
                _options.JsonSerializerOptions), additionalHeaders);
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