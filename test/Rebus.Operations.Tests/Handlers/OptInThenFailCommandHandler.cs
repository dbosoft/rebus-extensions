using System;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

/// <summary>
/// Opts in to cancellation (registers a token) and then fails with an ordinary
/// exception. The task must end failed and its registry entry must be cleaned up
/// rather than leaking.
/// </summary>
public class OptInThenFailCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<OptInThenFailCommand>>
{
    public Task Handle(OperationTask<OptInThenFailCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        _ = messaging.GetCancellationToken(message);
        throw new InvalidOperationException("handler failed after opting in to cancellation");
    }
}
