using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;

public static class OperationsSetup
{
    public static async Task<IBus> SubscribeEvents(IBus bus, WorkflowOptions options)
    {
        if (options.DispatchMode == WorkflowEventDispatchMode.Send)
            return bus;

        if (!string.IsNullOrWhiteSpace(options.EventDestination))
        {
            await bus.Advanced.Topics.Subscribe(options.EventDestination);
            return bus;
        }
        
        await bus.Subscribe<OperationStatusEvent>();
        await bus.Subscribe<OperationTaskAcceptedEvent>();
        await bus.Subscribe<OperationTaskProgressEvent>();
        await bus.Subscribe<OperationTaskStatusEvent>();
        await bus.Subscribe<OperationTimeoutEvent>();

        return bus;
    }
}