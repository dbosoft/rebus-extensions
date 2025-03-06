using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class StepTwoCommandHandler(
    ITaskMessaging messaging,
    TestTracer tracer)
    : IHandleMessages<OperationTask<StepTwoCommand>>
{
    public bool Throws { get; set; }


    public static bool Called { get; set; }

    public Task Handle(OperationTask<StepTwoCommand> message)
    {
        tracer.Trace(this, nameof(Handle), message);
        Called = true;

        if (Throws)
            throw new Exception("Failed");

        return messaging.CompleteTask(message);
    }
}