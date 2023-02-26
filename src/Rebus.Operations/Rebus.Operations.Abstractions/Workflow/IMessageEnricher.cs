using Dbosoft.Rebus.Operations.Commands;

namespace Dbosoft.Rebus.Operations.Workflow;

public interface IMessageEnricher
{
    object? EnrichTaskAcceptedReply<T>(OperationTaskSystemMessage<T> taskMessage) where T : class, new();
}