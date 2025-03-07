using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class FinalStepCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<FinalStepCommand>>
{
    public async Task Handle(OperationTask<FinalStepCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        await messaging.ProgressMessage(message, $"{nameof(FinalStepCommandHandler)}-1");
        await messaging.ProgressMessage(message, $"{nameof(FinalStepCommandHandler)}-2");
        await messaging.CompleteTask(message);
    }
}
