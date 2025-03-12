using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests.Sagas;

public class ParallelSagaData : TaskWorkflowSagaData
{
    public ISet<Guid> TaskIds { get; set; } = new HashSet<Guid>();
}
