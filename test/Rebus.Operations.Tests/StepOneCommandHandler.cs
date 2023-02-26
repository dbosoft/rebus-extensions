using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class StepOneCommandHandler : IHandleMessages<OperationTask<StepOneCommand>>
{
    private readonly IBus _bus;

    public StepOneCommandHandler(IBus bus)
    {
        _bus = bus;
    }

    public static bool Called { get; set; }

    public Task Handle(OperationTask<StepOneCommand> message)
    {
        Called = true;
        return _bus.CompleteTask(message);
    }
}