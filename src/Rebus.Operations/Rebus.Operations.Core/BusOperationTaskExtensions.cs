using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;

public static class BusOperationTaskExtensions
{
    public static Task SendWorkflowEvent(this IBus bus, WorkflowOptions options, object eventMessage,
        IDictionary<string, string>? additionalHeaders = null)
    {
        if (string.IsNullOrWhiteSpace(options.EventDestination))
        {
            return options.DispatchMode switch
            {
                WorkflowEventDispatchMode.Publish => bus.Publish(eventMessage, additionalHeaders),
                WorkflowEventDispatchMode.Send => bus.Send(eventMessage, additionalHeaders),
                _ => throw new ArgumentOutOfRangeException(nameof(options))
            };
        }
        
        return options.DispatchMode switch
        {
            WorkflowEventDispatchMode.Publish => bus.Advanced.Topics.Publish(options.EventDestination, eventMessage, additionalHeaders),
            WorkflowEventDispatchMode.Send => bus.Advanced.Routing.Send(options.EventDestination, eventMessage, additionalHeaders),
            _ => throw new ArgumentOutOfRangeException(nameof(options))
        };
    }
    
}