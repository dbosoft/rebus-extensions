using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;

public static class OperationsSetup
{
    public static async Task<IBus> SubscribeEvents(IBus bus)
    {
        await bus.Subscribe<OperationStatusEvent>();
        await bus.Subscribe<OperationTaskAcceptedEvent>();
        await bus.Subscribe<OperationTaskProgressEvent>();
        await bus.Subscribe<OperationTaskStatusEvent>();
        await bus.Subscribe<OperationTimeoutEvent>();

        return bus;
    }
}