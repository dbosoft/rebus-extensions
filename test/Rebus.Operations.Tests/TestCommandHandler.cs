using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandler : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly ITaskMessaging _messaging;
    public bool Called { get; private set; }

    public TestCommandHandler(ITaskMessaging operationMessaging)
    {
        _messaging = operationMessaging;
    }
    
    public Task Handle(OperationTask<TestCommand> message)
    {
        Called = true;
        return _messaging.CompleteTask(message);
    }
}