using Dbosoft.Rebus.Operations.Commands;

namespace Dbosoft.Rebus.Operations.Workflow;

public class DefaultMessageEnricher : IMessageEnricher
{
    public object? EnrichTaskAcceptedReply<T>(OperationTaskSystemMessage<T> taskMessage) where T : class, new()
    {
        return null;
    }
}