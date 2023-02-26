using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class StepTwoCommandHandler : IHandleMessages<OperationTask<StepTwoCommand>>
{
    private readonly IBus _bus;

    public StepTwoCommandHandler(IBus bus)
    {
        _bus = bus;
    }

    public static bool Called { get; set; }

    public Task Handle(OperationTask<StepTwoCommand> message)
    {
        Called = true;
        return _bus.CompleteTask(message);
    }
}