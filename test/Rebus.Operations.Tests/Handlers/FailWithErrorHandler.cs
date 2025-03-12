using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class FailWithErrorHandler<TCommand>(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<TCommand>>
    where TCommand : class, new()

{
    public async Task Handle(OperationTask<TCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        await messaging.FailTask(message, "TEST ERROR!");
    }
}
