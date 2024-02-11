using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class StepTwoCommandHandler : IHandleMessages<OperationTask<StepTwoCommand>>
{
    private readonly ITaskMessaging _messaging;
    private readonly bool _throws;

    public StepTwoCommandHandler(ITaskMessaging messaging, bool throws)
    {
        _messaging = messaging;
        _throws = throws;
    }

    public static bool Called { get; set; }

    public Task Handle(OperationTask<StepTwoCommand> message)
    {
        Called = true;

        if (_throws)
            throw new Exception("Failed");

        return _messaging.CompleteTask(message);
    }
}