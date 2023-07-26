using System.Collections.Generic;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;

namespace Dbosoft.Rebus.Operations.Workflow;

public interface IMessageEnricher
{
    object? EnrichTaskAcceptedReply<T>(OperationTaskSystemMessage<T> taskMessage) where T : class, new();

    IDictionary<string, string>? EnrichHeadersFromIncomingSystemMessage<T>(OperationTaskSystemMessage<T> taskMessage,
        IDictionary<string, string> systemMessageHeaders);
    
    IDictionary<string, string>? EnrichHeadersOfOutgoingSystemMessage(object taskMessage,
        IDictionary<string, string>? previousHeaders);
    
    IDictionary<string, string>? EnrichHeadersOfStatusEvent(OperationStatusEvent operationStatusEvent,
        IDictionary<string, string>? previousHeaders);
    
    IDictionary<string, string>? EnrichHeadersOfTaskStatusEvent(OperationTaskStatusEvent operationStatusEvent,
        IDictionary<string, string>? previousHeaders);
}
