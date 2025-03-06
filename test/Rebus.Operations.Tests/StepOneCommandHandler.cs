using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class StepOneCommandHandler(
    ITaskMessaging messaging,
    TestTracer tracer)
    : IHandleMessages<OperationTask<StepOneCommand>>
{
    public static bool Called { get; set; }

    public Task Handle(OperationTask<StepOneCommand> message)
    {
        Called = true;
        tracer.Trace(this, nameof(Handle), message);
        return messaging.CompleteTask(message);
    }
}