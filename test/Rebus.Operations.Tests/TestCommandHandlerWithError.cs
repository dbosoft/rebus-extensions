using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandlerWithError : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly IOperationMessaging _messaging;
    private readonly bool _throws;

    public TestCommandHandlerWithError(bool throws, IOperationMessaging messaging)
    {
        _throws = throws;
        _messaging = messaging;
    }
    
    public async Task Handle(OperationTask<TestCommand> message)
    {
        if (_throws)
            throw new InvalidOperationException();
        
        await _messaging.FailTask(message, "error");
    }
}