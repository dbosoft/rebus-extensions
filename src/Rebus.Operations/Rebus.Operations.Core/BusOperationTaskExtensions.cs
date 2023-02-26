using System;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Bus;
using Rebus.Transport;

namespace Dbosoft.Rebus.Operations;

public static class BusOperationTaskExtensions
{
    public static Task FailTask(this IBus bus, IOperationTaskMessage message, string errorMessage)
    {
        return FailTask(bus, message, new ErrorData { ErrorMessage = errorMessage });
    }

    public static Task FailTask(this IBus bus, IOperationTaskMessage message, ErrorData error)
    {
        return bus.Publish(
            OperationTaskStatusEvent.Failed(
                message.OperationId, message.InitiatingTaskId,
                message.TaskId, error));
    }


    public static Task CompleteTask(this IBus bus, IOperationTaskMessage message)
    {
        return bus.Publish(
            OperationTaskStatusEvent.Completed(
                message.OperationId, message.InitiatingTaskId, message.TaskId));
    }

    public static Task CompleteTask(this IBus bus, IOperationTaskMessage message, object responseMessage)
    {
        return bus.Publish(
            OperationTaskStatusEvent.Completed(
                message.OperationId, message.InitiatingTaskId, message.TaskId, responseMessage));
    }


    public static async Task ProgressMessage(this IBus bus, IOperationTaskMessage message, object data)
    {
        using var scope = new RebusTransactionScope();


        await bus.Publish(new OperationTaskProgressEvent
        {
            Id = Guid.NewGuid(),
            OperationId = message.OperationId,
            TaskId = message.TaskId,
            Data = data,
            Timestamp = DateTimeOffset.UtcNow
        }).ConfigureAwait(false);

        // commit it like this
        await scope.CompleteAsync().ConfigureAwait(false);
    }
}