using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests.Sagas;

public class FailWithExceptionSaga(
    IWorkflow workflowEngine,
    TestTrace trace) :
    OperationTaskWorkflowSaga<SagaCommand, FailWithExceptionSagaData>(workflowEngine)
{
    protected override Task Initiated(SagaCommand message)
    {
        trace.Trace(this, nameof(Initiated), message);
        throw new InvalidOperationException("TEST EXCEPTION!");
    }
}