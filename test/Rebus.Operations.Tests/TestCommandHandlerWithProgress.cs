using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandlerWithProgress : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly IBus _bus;
    public TestCommandHandlerWithProgress(IBus bus)
    {
        _bus = bus;
    }
    
    public async Task Handle(OperationTask<TestCommand> message)
    {
        await _bus.ProgressMessage(message, "progressData");
        await Task.Delay(500);
        
        await _bus.CompleteTask(message);
    }
}