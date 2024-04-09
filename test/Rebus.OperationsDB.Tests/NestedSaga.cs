using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class NestedSaga : OperationTaskWorkflowSaga<NestedSagaCommand, NestedSagaData>, 
    IHandleMessages<OperationTaskStatusEvent<SubCommand2>>,
    IHandleMessages<OperationTaskStatusEvent<SubCommand3>>,
    IHandleMessages<OperationTaskStatusEvent<NestedNestedSagaCommand>>
{
    protected override void CorrelateMessages(ICorrelationConfig<NestedSagaData> config)
    {
        base.CorrelateMessages(config);

        config.Correlate<OperationTaskStatusEvent<SubCommand2>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<SubCommand3>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<NestedNestedSagaCommand>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
    }

    protected override async Task Initiated(NestedSagaCommand message)
    {
        await StartNewTask<SubCommand2>();
        await StartNewTask<SubCommand3>();
        await StartNewTask<NestedNestedSagaCommand>();
    }
    
    public async Task Handle(OperationTaskStatusEvent<NestedNestedSagaCommand> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SagaCompleted = true;

            if (Data.SagaCompleted && Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete();
        });
    }

    public async Task Handle(OperationTaskStatusEvent<SubCommand2> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SubCommand2Completed = true;

            if (Data.SagaCompleted && Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete();
        });
    }

    public async Task Handle(OperationTaskStatusEvent<SubCommand3> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SubCommand3Completed = true;

            if (Data.SagaCompleted && Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete();
        });
    }

    public NestedSaga(IWorkflow workflowEngine) : base(workflowEngine)
    {
    }
}