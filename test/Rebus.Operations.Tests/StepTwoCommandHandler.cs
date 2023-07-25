using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class StepTwoCommandHandler : IHandleMessages<OperationTask<StepTwoCommand>>
{
    private readonly IOperationMessaging _messaging;

    public StepTwoCommandHandler(IOperationMessaging messaging)
    {
        _messaging = messaging;
    }

    public static bool Called { get; set; }

    public Task Handle(OperationTask<StepTwoCommand> message)
    {
        Called = true;
        return _messaging.CompleteTask(message);
    }
}