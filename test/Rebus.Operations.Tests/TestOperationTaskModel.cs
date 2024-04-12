namespace Dbosoft.Rebus.Operations.Tests;

public class TestOperationTaskModel : IOperationTask
{
    public Guid Id { get; set; }
    public Guid OperationId { get; set;  }
    public Guid InitiatingTaskId { get; set; }
    public OperationTaskStatus Status { get; set; }
    public DateTimeOffset Created { get; set; }
}