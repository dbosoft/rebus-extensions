using Dbosoft.Rebus.Operations;

namespace Dbosoft.Rebus.OperationsDB.Tests.Models;

public class OperationTaskModel : OperationAndTaskModel, IOperationTask
{
    public OperationModel? Operation { get; set; }
    public Guid OperationId { get; set; }
    public Guid InitiatingTaskId { get; set; }
    public OperationTaskStatus Status { get; set; }
    public int Progress { get; set; }
    
}