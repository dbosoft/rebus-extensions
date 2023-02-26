using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandler : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly IBus _bus;
    public bool Called { get; private set; }

    public TestCommandHandler(IBus bus)
    {
        _bus = bus;
    }
    
    public Task Handle(OperationTask<TestCommand> message)
    {
        Called = true;
        return _bus.CompleteTask(message);
    }
}