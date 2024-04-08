using Dbosoft.Rebus.Operations;

namespace Dbosoft.Rebus.OperationsDB.Tests.Models;

public class OperationAndTaskModel
{
    public Guid Id { get; set; }
    
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset LastUpdate { get; set; }
    
}

public class OperationModel : OperationAndTaskModel, IOperation
{
    public List<OperationTaskModel>? Tasks { get; set; }
    public OperationStatus Status { get; set;  }
}

public class OperationTaskModel : OperationAndTaskModel, IOperationTask
{
    public OperationModel? Operation { get; set; }
    public Guid OperationId { get; set; }
    public Guid InitiatingTaskId { get; set; }
    public OperationTaskStatus Status { get; set; }

}