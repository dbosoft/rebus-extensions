using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandler : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly IOperationMessaging _operationMessaging;
    public bool Called { get; private set; }

    public TestCommandHandler(IOperationMessaging operationMessaging)
    {
        _operationMessaging = operationMessaging;
    }
    
    public Task Handle(OperationTask<TestCommand> message)
    {
        Called = true;
        return _operationMessaging.CompleteTask(message);
    }
}