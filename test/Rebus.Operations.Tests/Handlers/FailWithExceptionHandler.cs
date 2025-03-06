using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class FailWithExceptionHandler<TCommand>(
    ITaskMessaging messaging,
    TestTracer tracer)
    : IHandleMessages<OperationTask<TCommand>>
    where TCommand : class, new()

{
    public Task Handle(OperationTask<TCommand> message)
    {
        tracer.Trace(this, nameof(Handle), message);
        throw new InvalidOperationException("TEST EXCEPTION!");
    }
}
