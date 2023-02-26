using System.Runtime.InteropServices.ComTypes;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Tests;

public class MultiStepSaga : 
    OperationTaskWorkflowSaga<MultiStepCommand, MultiStepSagaData>,
    IHandleMessages<OperationTaskStatusEvent<StepOneCommand>>,
    IHandleMessages<OperationTaskStatusEvent<StepTwoCommand>>
{
    public MultiStepSaga(IWorkflow workflowEngine) : base(workflowEngine)
    {
    }

    protected override void CorrelateMessages(ICorrelationConfig<MultiStepSagaData> config)
    {
        config.Correlate<OperationTaskStatusEvent<StepOneCommand>>(m => m.InitiatingTaskId, d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<StepTwoCommand>>(m => m.InitiatingTaskId, d => d.SagaTaskId);

        base.CorrelateMessages(config);
    }

    protected override Task Initiated(MultiStepCommand message)
    {
        return StartNewTask<StepOneCommand>().AsTask();
    }

    public Task Handle(OperationTaskStatusEvent<StepOneCommand> message)
    {
        return FailOrRun(message, 
            () => StartNewTask<StepTwoCommand>().AsTask());
    }

    public Task Handle(OperationTaskStatusEvent<StepTwoCommand> message)
    {
        return FailOrRun(message, () => Complete());
    }
}