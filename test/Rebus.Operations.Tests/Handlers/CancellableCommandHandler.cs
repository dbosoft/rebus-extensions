using System.Threading;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

/// <summary>
/// A handler that opts in to cancellation: it observes the cancellation token and
/// throws when it is tripped, leaving the cancellation pipeline step to report the
/// task as cancelled.
/// </summary>
public class CancellableCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<CancellableCommand>>
{
    public static TaskCompletionSource<bool> Started =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static volatile bool TokenObserved;

    public static void Reset()
    {
        Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        TokenObserved = false;
    }

    public async Task Handle(OperationTask<CancellableCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        var token = messaging.GetCancellationToken(message);
        Started.TrySetResult(true);
        try
        {
            await Task.Delay(Timeout.Infinite, token);
        }
        catch (OperationCanceledException)
        {
            TokenObserved = true;
            throw;
        }
    }
}
