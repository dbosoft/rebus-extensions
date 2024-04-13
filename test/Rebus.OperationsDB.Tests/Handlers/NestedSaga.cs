using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Dbosoft.Rebus.OperationsDB.Tests.Commands;
using JetBrains.Annotations;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

[UsedImplicitly]
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
        await StartNewTask<SubCommand2>().ConfigureAwait(false);
        await StartNewTask<SubCommand3>().ConfigureAwait(false);
        await StartNewTask<NestedNestedSagaCommand>().ConfigureAwait(false);
    }

    public async Task Handle(OperationTaskStatusEvent<NestedNestedSagaCommand> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SagaCompleted = true;

            if (Data.SagaCompleted && Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task Handle(OperationTaskStatusEvent<SubCommand2> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SubCommand2Completed = true;

            if (Data.SagaCompleted && Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task Handle(OperationTaskStatusEvent<SubCommand3> message)
    {
        await FailOrRun(message, async () =>
        {
            Data.SubCommand3Completed = true;

            if (Data.SagaCompleted && Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public NestedSaga(IWorkflow workflowEngine) : base(workflowEngine)
    {
    }
}