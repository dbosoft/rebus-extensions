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
            await bus.Advanced.Topics.Subscribe(options.EventDestination).ConfigureAwait(false);
            return bus;
        }
        
        await bus.Subscribe<OperationStatusEvent>().ConfigureAwait(false);
        await bus.Subscribe<OperationTaskAcceptedEvent>().ConfigureAwait(false);
        await bus.Subscribe<OperationTaskProgressEvent>().ConfigureAwait(false);
        await bus.Subscribe<OperationTaskStatusEvent>().ConfigureAwait(false);
        await bus.Subscribe<OperationTimeoutEvent>().ConfigureAwait(false);

        return bus;
    }
}