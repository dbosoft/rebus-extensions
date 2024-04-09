using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class InitialSaga : OperationTaskWorkflowSaga<InitialSagaCommand, InitialSagaData>, 
    IHandleMessages<OperationTaskStatusEvent<NestedSagaCommand>>,
    IHandleMessages<OperationTaskStatusEvent<SubCommand1>>
    
{
    public InitialSaga(IWorkflow workflowEngine) : base(workflowEngine)
    {
    }
    
    protected override void CorrelateMessages(ICorrelationConfig<InitialSagaData> config)
    {
        base.CorrelateMessages(config);

        config.Correlate<OperationTaskStatusEvent<NestedSagaCommand>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<SubCommand1>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
    }

    protected override async Task Initiated(InitialSagaCommand message)
    {
        await StartNewTask<NestedSagaCommand>();
        await StartNewTask<SubCommand1>();
    }
    

    public async Task Handle(OperationTaskStatusEvent<SubCommand1> message)
    {
        await FailOrRun<SubCommand1>(message, async () =>
        {
            Data.SubCommand1Completed = true;

            if (Data.SubCommand1Completed && Data.SagaCompleted)
                await Complete();
        });
    }

    public async Task Handle(OperationTaskStatusEvent<NestedSagaCommand> message)
    {
        await FailOrRun<NestedSagaCommand>(message, async () =>
        {
            Data.SagaCompleted = true;

            if (Data.SubCommand1Completed && Data.SagaCompleted)
                await Complete();
        });
    }


}