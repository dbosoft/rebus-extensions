using Dbosoft.Rebus.Operations;

namespace Dbosoft.Rebus.OperationsDB.Tests.Models;

public class OperationModel : OperationAndTaskModel, IOperation
{
    public List<OperationTaskModel>? Tasks { get; set; }
    public OperationStatus Status { get; set;  }
    

}