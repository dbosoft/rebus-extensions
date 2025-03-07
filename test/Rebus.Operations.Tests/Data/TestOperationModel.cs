namespace Dbosoft.Rebus.Operations.Tests.Data;

public class TestOperationModel : IOperation
{
    public Guid Id { get; set; }

    public OperationStatus Status { get; set;  }

    public DateTimeOffset CreatedAt { get; set; }

    public object? Data { get; set; }
}
