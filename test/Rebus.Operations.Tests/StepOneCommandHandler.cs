using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class StepOneCommandHandler : IHandleMessages<OperationTask<StepOneCommand>>
{
    private readonly IOperationMessaging _messaging;

    public StepOneCommandHandler(IOperationMessaging messaging)
    {
        _messaging = messaging;
    }

    public static bool Called { get; set; }

    public Task Handle(OperationTask<StepOneCommand> message)
    {
        Called = true;
        return _messaging.CompleteTask(message);
    }
}