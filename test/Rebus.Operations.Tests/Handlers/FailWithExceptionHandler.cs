using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class FailWithExceptionHandler<TCommand>(
#pragma warning disable CS9113 // Parameter is unread.
    ITaskMessaging messaging,
#pragma warning restore CS9113 // Parameter is unread.
    TestTrace trace)
    : IHandleMessages<OperationTask<TCommand>>
    where TCommand : class, new()

{
    public Task Handle(OperationTask<TCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        throw new InvalidOperationException("TEST EXCEPTION!");
    }
}
