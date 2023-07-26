using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Bus;
using Rebus.Pipeline;

namespace Dbosoft.Rebus.Operations.Workflow;

public class RebusOperationMessaging : IOperationMessaging
{
    private readonly IBus _bus;
    private readonly IMessageEnricher _messageEnricher;
    private readonly WorkflowOptions _options;

    public RebusOperationMessaging(IBus bus, 
        IOperationDispatcher operationDispatcher, 
        IOperationTaskDispatcher taskDispatcher, 
        IMessageEnricher messageEnricher,
        WorkflowOptions options)
    {
        _bus = bus;
        _messageEnricher = messageEnricher;
        _options = options;
        OperationDispatcher = operationDispatcher;
        TaskDispatcher = taskDispatcher;
    }

    private IDictionary<string, string>? JoinHeaders(IDictionary<string, string>? one,
        IDictionary<string, string>? another)
    {
        if (one == null)
            return another;

        if (another == null)
            return one;
        
        var result = new []{one, another}.SelectMany(dict => dict)
            .ToLookup(pair => pair.Key, pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.First());

        return result.Count == 0 ? null : result;
    }

    public virtual Task DispatchTaskMessage(object command, IOperationTask task, IDictionary<string,string>? additionalHeaders = null)
    {
        var messageType = command.GetType();
        var outboundMessage = Activator.CreateInstance(
            typeof(OperationTaskSystemMessage<>).MakeGenericType(messageType),
            command, task.OperationId, task.InitiatingTaskId, task.Id);

        var taskHeaders = _messageEnricher.EnrichHeadersOfOutgoingSystemMessage(command,
            JoinHeaders(additionalHeaders, MessageContext.Current.Headers));
        return _bus.SendLocal(outboundMessage, taskHeaders);
    }

    public Task DispatchTaskStatusEventAsync(string commandType, OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null)
    {
        var genericType = typeof(OperationTaskStatusEvent<>);
        var wrappedCommandType = genericType.MakeGenericType(Type.GetType(commandType)
                                                             ?? throw new InvalidOperationException(
                                                                 $"Unknown task command type '{commandType}'."));

        var commandInstance = Activator.CreateInstance(wrappedCommandType, message);
        
        var eventHeaders = _messageEnricher.EnrichHeadersOfTaskStatusEvent(message,
            JoinHeaders(additionalHeaders, MessageContext.Current.Headers));
        return  _bus.SendLocal(commandInstance, eventHeaders);
    }

    public Task DispatchTaskStatusEventAsync(OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null)
    {
        var eventHeaders = _messageEnricher.EnrichHeadersOfTaskStatusEvent(message,
            JoinHeaders(additionalHeaders, MessageContext.Current.Headers));
        return _bus.SendWorkflowEvent(_options, message, eventHeaders);
    }

    public Task DispatchOperationStatusEventAsync(OperationStatusEvent message, IDictionary<string,string>? additionalHeaders = null)
    {
        var eventHeaders = _messageEnricher.EnrichHeadersOfStatusEvent(message,
            JoinHeaders(additionalHeaders, MessageContext.Current.Headers));
        return _bus.SendWorkflowEvent(_options, message, eventHeaders);
    }

    public IOperationDispatcher OperationDispatcher { get; }
    public IOperationTaskDispatcher TaskDispatcher { get; }
    
    
}