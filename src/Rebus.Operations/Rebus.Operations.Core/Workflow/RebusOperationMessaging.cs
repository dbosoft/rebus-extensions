using System;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations.Workflow;

public class RebusOperationMessaging : IOperationMessaging
{
    private readonly IBus _bus;

    public RebusOperationMessaging(IBus bus, 
        IOperationDispatcher operationDispatcher, IOperationTaskDispatcher taskDispatcher)
    {
        _bus = bus;
        OperationDispatcher = operationDispatcher;
        TaskDispatcher = taskDispatcher;
    }

    public virtual Task DispatchTaskMessage(object command, IOperationTask task)
    {
        var messageType = command.GetType();
        var outboundMessage = Activator.CreateInstance(
            typeof(OperationTaskSystemMessage<>).MakeGenericType(messageType),
            command, task.OperationId, task.InitiatingTaskId, task.Id);

        return _bus.SendLocal(outboundMessage);
    }

    public Task DispatchTaskStatusEventAsync(string commandType, OperationTaskStatusEvent message)
    {
        var genericType = typeof(OperationTaskStatusEvent<>);
        var wrappedCommandType = genericType.MakeGenericType(Type.GetType(commandType)
                                                             ?? throw new InvalidOperationException(
                                                                 $"Unknown task command type '{commandType}'."));

        var commandInstance = Activator.CreateInstance(wrappedCommandType, message);
        return _bus.SendLocal(commandInstance);
    }

    public Task DispatchTaskStatusEventAsync(OperationTaskStatusEvent message)
    {
        return _bus.Publish(message);
    }

    public Task DispatchOperationStatusEventAsync(OperationStatusEvent message)
    {
        return _bus.Publish(message);
    }

    public IOperationDispatcher OperationDispatcher { get; }
    public IOperationTaskDispatcher TaskDispatcher { get; }
}