namespace Dbosoft.Rebus.Operations.Tests;

public class TestOperationModel : IOperation
{
    public Guid Id { get; set; }
    public OperationStatus Status { get; set;  }
}