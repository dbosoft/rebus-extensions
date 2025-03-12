using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Tests.Sagas;

public class ParallelSaga(
    IWorkflow workflowEngine,
    TestTrace trace)
    : OperationTaskWorkflowSaga<SagaCommand, ParallelSagaData>(workflowEngine),
        IHandleMessages<OperationTaskStatusEvent<WithoutResponseCommand>>
{
    protected override async Task Initiated(SagaCommand message)
    {
        trace.Trace(this, nameof(Initiated), message);
        Data.TaskIds.Add((await StartNewTask<WithoutResponseCommand>())!.Id);
        Data.TaskIds.Add((await StartNewTask(typeof(WithoutResponseCommand)))!.Id);
        Data.TaskIds.Add((await StartNewTask(new WithoutResponseCommand()))!.Id);
    }

    public Task Handle(OperationTaskStatusEvent<WithoutResponseCommand> message)
    {
        return FailOrRun(message, async () =>
        {
            trace.Trace(this, nameof(Handle), message);
            
            Data.TaskIds.Remove(message.TaskId);
            if (Data.TaskIds.Count == 0)
                await Complete();
        });
    }

    protected override void CorrelateMessages(ICorrelationConfig<ParallelSagaData> config)
    {
        base.CorrelateMessages(config);
        config.Correlate<OperationTaskStatusEvent<WithoutResponseCommand>>(
            m => m.InitiatingTaskId, d => d.SagaTaskId);
    }
}
