using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class SimpleCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<SimpleCommand>>
{
    public async Task Handle(OperationTask<SimpleCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        await messaging.CompleteTask(message);
    }
}
