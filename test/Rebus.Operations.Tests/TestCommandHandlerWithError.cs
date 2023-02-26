using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandlerWithError : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly IBus _bus;
    private readonly bool _throws;

    public TestCommandHandlerWithError(IBus bus, bool throws)
    {
        _bus = bus;
        _throws = throws;
    }
    
    public async Task Handle(OperationTask<TestCommand> message)
    {
        if (_throws)
            throw new InvalidOperationException();
        
        await _bus.FailTask(message, "error");
    }
}