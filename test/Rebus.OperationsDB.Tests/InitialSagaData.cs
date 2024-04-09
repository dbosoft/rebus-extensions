using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class InitialSagaData : TaskWorkflowSagaData
{
    public bool SubCommand1Completed { get; set; }
    public bool SagaCompleted { get; set; }
}