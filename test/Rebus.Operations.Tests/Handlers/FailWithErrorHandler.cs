using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class FailWithErrorHandler<TCommand>(
    ITaskMessaging messaging,
    TestTracer tracer)
    : IHandleMessages<OperationTask<TCommand>>
    where TCommand : class, new()

{
    public async Task Handle(OperationTask<TCommand> message)
    {
        tracer.Trace(this, nameof(Handle), message);
        await messaging.FailTask(message, "TEST ERROR!");
    }
}
