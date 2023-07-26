using System.Collections.Generic;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;

namespace Dbosoft.Rebus.Operations.Workflow;

public class DefaultMessageEnricher : IMessageEnricher
{
    public object? EnrichTaskAcceptedReply<T>(OperationTaskSystemMessage<T> taskMessage) where T : class, new()
    {
        return null;
    }

    public IDictionary<string, string>? EnrichHeadersFromIncomingSystemMessage<T>(
        OperationTaskSystemMessage<T> taskMessage, IDictionary<string, string> systemMessageHeaders)
    {
        return null;
    }

    public IDictionary<string, string>? EnrichHeadersOfOutgoingSystemMessage(object taskMessage,
        IDictionary<string, string>? previousHeaders)
    {
        return null;
    }

    public IDictionary<string, string>? EnrichHeadersOfStatusEvent(OperationStatusEvent operationStatusEvent, IDictionary<string, string>? previousHeaders)
    {
        return null;
    }

    public IDictionary<string, string>? EnrichHeadersOfTaskStatusEvent(OperationTaskStatusEvent operationStatusEvent,
        IDictionary<string, string>? previousHeaders)
    {
        return null;
    }
}