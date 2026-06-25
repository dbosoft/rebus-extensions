using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

/// <summary>
/// Throws an <see cref="OperationCanceledException"/> that is NOT caused by the
/// operation's cancellation registry (it uses its own source, as a handler doing
/// timeout control might). The cancellation pipeline step must not misclassify this
/// as a task cancellation.
/// </summary>
public class SelfCancelCommandHandler : IHandleMessages<OperationTask<SelfCancelCommand>>
{
    private readonly TestTrace _trace;

    // The (ITaskMessaging, TestTrace) signature is required by the test harness'
    // handler factory; this handler does not need the messaging instance.
    public SelfCancelCommandHandler(ITaskMessaging messaging, TestTrace trace)
    {
        _ = messaging;
        _trace = trace;
    }

    public Task Handle(OperationTask<SelfCancelCommand> message)
    {
        _trace.Trace(this, nameof(Handle), message);
        using var ownSource = new CancellationTokenSource();
        ownSource.Cancel();
        ownSource.Token.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
