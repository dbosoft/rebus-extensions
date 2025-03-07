using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests.Sagas;

public class FailWithErrorSaga(
    IWorkflow workflowEngine,
    TestTrace trace) :
    OperationTaskWorkflowSaga<SagaCommand, FailWithErrorSagaData>(workflowEngine)
{
    protected override async Task Initiated(SagaCommand message)
    {
        trace.Trace(this, nameof(Initiated), message);
        await Fail("TEST ERROR!");
    }
}
