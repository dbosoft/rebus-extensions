using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Tests.Sagas;

// A saga that orchestrates a single cancellable child task. Used to prove that a
// child task's cancellation propagates up through the saga to the operation.
public class CancellableSaga(
    IWorkflow workflowEngine,
    TestTrace trace) :
    OperationTaskWorkflowSaga<CancellableSagaCommand, CancellableSagaData>(workflowEngine),
    IHandleMessages<OperationTaskStatusEvent<CancellableCommand>>
{
    protected override async Task Initiated(CancellableSagaCommand message)
    {
        trace.Trace(this, nameof(Initiated), message);
        await StartNewTask<CancellableCommand>();
    }

    public Task Handle(OperationTaskStatusEvent<CancellableCommand> message)
    {
        return FailOrRun(message, async () =>
        {
            trace.Trace(this, nameof(Handle), message);
            await Complete();
        });
    }

    protected override void CorrelateMessages(ICorrelationConfig<CancellableSagaData> config)
    {
        config.Correlate<OperationTaskStatusEvent<CancellableCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);

        base.CorrelateMessages(config);
    }
}
