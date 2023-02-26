using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Workflow;

public class EmptyOperationTaskStatusEventHandler<TMessage> : IHandleMessages<OperationTaskStatusEvent<TMessage>> 
    where TMessage : class, new()
{
    public Task Handle(OperationTaskStatusEvent<TMessage> message)
    {
        return Task.CompletedTask;
    }
}