using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Workflow;

public class EmptyOperationStatusEventHandler : IHandleMessages<OperationStatusEvent> 

{
    public Task Handle(OperationStatusEvent message)
    {
        return Task.CompletedTask;
    }
}