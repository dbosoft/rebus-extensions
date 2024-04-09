namespace Dbosoft.Rebus.OperationsDB.Tests.Models;

public class OperationAndTaskModel
{
    public Guid Id { get; set; }
    
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset LastUpdate { get; set; }
    
}