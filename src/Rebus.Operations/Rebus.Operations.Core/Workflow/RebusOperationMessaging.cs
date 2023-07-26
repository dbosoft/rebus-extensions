using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations.Workflow;

public class RebusOperationMessaging : IOperationMessaging
{
    private readonly IBus _bus;
    private readonly WorkflowOptions _options;

    public RebusOperationMessaging(IBus bus, 
        IOperationDispatcher operationDispatcher, 
        IOperationTaskDispatcher taskDispatcher, 
        WorkflowOptions options)
    {
        _bus = bus;
        _options = options;
        OperationDispatcher = operationDispatcher;
        TaskDispatcher = taskDispatcher;
    }

    public virtual Task DispatchTaskMessage(object command, IOperationTask task, IDictionary<string,string>? additionalHeaders = null)
    {
        var messageType = command.GetType();
        var outboundMessage = Activator.CreateInstance(
            typeof(OperationTaskSystemMessage<>).MakeGenericType(messageType),
            command, task.OperationId, task.InitiatingTaskId, task.Id);

        return _bus.SendLocal(outboundMessage);
    }

    public Task DispatchTaskStatusEventAsync(string commandType, OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null)
    {
        var genericType = typeof(OperationTaskStatusEvent<>);
        var wrappedCommandType = genericType.MakeGenericType(Type.GetType(commandType)
                                                             ?? throw new InvalidOperationException(
                                                                 $"Unknown task command type '{commandType}'."));

        var commandInstance = Activator.CreateInstance(wrappedCommandType, message);
        return  _bus.SendLocal(commandInstance, additionalHeaders);
    }

    public Task DispatchTaskStatusEventAsync(OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null)
    {
        return _bus.SendWorkflowEvent(_options, message, additionalHeaders);
    }

    public Task DispatchOperationStatusEventAsync(OperationStatusEvent message, IDictionary<string,string>? additionalHeaders = null)
    {
        return _bus.SendWorkflowEvent(_options, message, additionalHeaders);
    }

    public IOperationDispatcher OperationDispatcher { get; }
    public IOperationTaskDispatcher TaskDispatcher { get; }
    
    
}