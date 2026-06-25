using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

/// <summary>
/// A handler that does not opt in to cancellation: it never asks for a cancellation
/// token, so a cancellation request cannot interrupt it and it completes normally.
/// A test-controlled gate keeps it running until the test releases it.
/// </summary>
public class IgnoreCancellationCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<IgnoreCancellationCommand>>
{
    public static TaskCompletionSource<bool> Started =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static TaskCompletionSource<bool> Release =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static void Reset()
    {
        Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public async Task Handle(OperationTask<IgnoreCancellationCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        Started.TrySetResult(true);
        await Release.Task;
        await messaging.CompleteTask(message);
    }
}
