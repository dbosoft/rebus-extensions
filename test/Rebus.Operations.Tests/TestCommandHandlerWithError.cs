using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandlerWithError : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly ITaskMessaging _messaging;
    public bool Throws { get; set; }

    public TestCommandHandlerWithError(ITaskMessaging messaging)
    {
        _messaging = messaging;
    }
    
    public async Task Handle(OperationTask<TestCommand> message)
    {
        if (Throws)
            throw new InvalidOperationException();
        
        await _messaging.FailTask(message, "error");
    }
}