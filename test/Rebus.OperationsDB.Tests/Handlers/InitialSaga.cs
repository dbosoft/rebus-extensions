using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Dbosoft.Rebus.OperationsDB.Tests.Commands;
using JetBrains.Annotations;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

[UsedImplicitly]
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
        await StartNewTask<NestedSagaCommand>().ConfigureAwait(false);
        await StartNewTask<SubCommand1>().ConfigureAwait(false);
    }


    public async Task Handle(OperationTaskStatusEvent<SubCommand1> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SubCommand1Completed = true;

            if (Data.SubCommand1Completed && Data.SagaCompleted)
                await Complete().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task Handle(OperationTaskStatusEvent<NestedSagaCommand> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SagaCompleted = true;

            if (Data.SubCommand1Completed && Data.SagaCompleted)
                await Complete().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }


}