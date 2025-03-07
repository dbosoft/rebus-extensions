using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class WithResponseCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<WithResponseCommand>>
{
    public async Task Handle(OperationTask<WithResponseCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        await messaging.ProgressMessage(message, $"{nameof(WithResponseCommandHandler)}-1");
        await messaging.ProgressMessage(message, $"{nameof(WithResponseCommandHandler)}-2");
        await messaging.CompleteTask(
            message,
            new WithResponseCommandResponse { Data = "test" });
    }
}
