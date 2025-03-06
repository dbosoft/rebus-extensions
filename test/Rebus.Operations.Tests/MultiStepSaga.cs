using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Tests;

public class MultiStepSaga(
    IWorkflow workflowEngine,
    TestTracer tracer) :
    OperationTaskWorkflowSaga<MultiStepCommand, MultiStepSagaData>(workflowEngine),
    IHandleMessages<OperationTaskStatusEvent<StepWithoutResponseCommand>>,
    IHandleMessages<OperationTaskStatusEvent<StepWithResponseCommand>>,
    IHandleMessages<OperationTaskStatusEvent<FinalStepCommand>>
{
    protected override async Task Initiated(MultiStepCommand message)
    {
        tracer.Trace(this, nameof(Initiated), message);
        await StartNewTask<StepWithoutResponseCommand>();
    }

    public Task Handle(OperationTaskStatusEvent<StepWithoutResponseCommand> message)
    {
        return FailOrRun(message, async () =>
        {
            tracer.Trace(this, nameof(Handle), message);
            await StartNewTask<StepWithResponseCommand>();
        });
    }

    public Task Handle(OperationTaskStatusEvent<StepWithResponseCommand> message)
    {
        return FailOrRun(message, async (StepWithResponseCommandResponse response) =>
        {
            tracer.Trace(this, nameof(Handle), message);
            await StartNewTask<FinalStepCommand>();
        });
    }

    public Task Handle(OperationTaskStatusEvent<FinalStepCommand> message)
    {
        return FailOrRun(message, async () =>
        {
            tracer.Trace(this, nameof(Handle), message);
            await Complete();
        });
    }

    protected override void CorrelateMessages(ICorrelationConfig<MultiStepSagaData> config)
    {
        config.Correlate<OperationTaskStatusEvent<StepWithoutResponseCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<StepWithResponseCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<FinalStepCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);

        base.CorrelateMessages(config);
    }
}
