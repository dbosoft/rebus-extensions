using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class NestedSagaData : TaskWorkflowSagaData
{
    public bool SubCommand2Completed { get; set; }
    public bool SubCommand3Completed { get; set; }
    public bool SagaCompleted  { get; set; }
}

public class NestedNestedSagaData : TaskWorkflowSagaData
{
    public bool SubCommand2Completed { get; set; }
    public bool SubCommand3Completed { get; set; }
    public bool SagaCompleted  { get; set; }
}