using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

public class InitialSagaData : TaskWorkflowSagaData
{
    public bool SubCommand1Completed { get; set; }
    public bool SagaCompleted { get; set; }
}