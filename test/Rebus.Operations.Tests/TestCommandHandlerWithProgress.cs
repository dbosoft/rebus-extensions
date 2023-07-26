using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandlerWithProgress : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly ITaskMessaging _messaging;
    public TestCommandHandlerWithProgress(ITaskMessaging messaging)
    {
        _messaging = messaging;
    }
    
    public async Task Handle(OperationTask<TestCommand> message)
    {
        await _messaging.ProgressMessage(message, "progressData");
        await Task.Delay(500);
        
        await _messaging.CompleteTask(message);
    }
}