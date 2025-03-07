using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Tests.Sagas;

public class MultiStepSaga(
    IWorkflow workflowEngine,
    TestTrace trace) :
    OperationTaskWorkflowSaga<SagaCommand, MultiStepSagaData>(workflowEngine),
    IHandleMessages<OperationTaskStatusEvent<WithoutResponseCommand>>,
    IHandleMessages<OperationTaskStatusEvent<WithResponseCommand>>,
    IHandleMessages<OperationTaskStatusEvent<FinalStepCommand>>
{
    protected override async Task Initiated(SagaCommand message)
    {
        trace.Trace(this, nameof(Initiated), message);
        await StartNewTask<WithoutResponseCommand>();
    }

    public Task Handle(OperationTaskStatusEvent<WithoutResponseCommand> message)
    {
        return FailOrRun(message, async () =>
        {
            trace.Trace(this, nameof(Handle), message);
            await StartNewTask<WithResponseCommand>();
        });
    }

    public Task Handle(OperationTaskStatusEvent<WithResponseCommand> message)
    {
        return FailOrRun(message, async (WithResponseCommandResponse response) =>
        {
            trace.Trace(this, nameof(Handle), message, response);
            await StartNewTask<FinalStepCommand>();
        });
    }

    public Task Handle(OperationTaskStatusEvent<FinalStepCommand> message)
    {
        return FailOrRun(message, async () =>
        {
            trace.Trace(this, nameof(Handle), message);
            await Complete();
        });
    }

    protected override void CorrelateMessages(ICorrelationConfig<MultiStepSagaData> config)
    {
        config.Correlate<OperationTaskStatusEvent<WithoutResponseCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<WithResponseCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<FinalStepCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);

        base.CorrelateMessages(config);
    }
}
