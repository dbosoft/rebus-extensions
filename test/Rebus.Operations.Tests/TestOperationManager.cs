using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestOperationManager : OperationManagerBase
{
    public static readonly Dictionary<Guid, TestOperationModel> Operations = new();
    public static readonly Dictionary<Guid, List<object>> Progress = new();

    public static void Reset()
    {
        Operations.Clear();
        Progress.Clear();
    }
    
    public override ValueTask<IOperation?> GetByIdAsync(Guid operationId)
    {
        return ValueTask.FromResult( 
            Operations.ContainsKey(operationId) 
                ? Operations[operationId] as IOperation : null);

    }

    public override ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command,
        object? additionalData, IDictionary<string,string>? additionalHeaders)
    {
        if (Operations.ContainsKey(operationId))
            return GetByIdAsync(operationId)!;
        
        var op = new TestOperationModel
        {
            Id = operationId
        };
        Operations.Add(operationId, op);
        
        return new ValueTask<IOperation>(op);
    }

    public override ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus, 
        object? additionalData, IDictionary<string,string>? messageHeaders)
    {
        if (!Operations.ContainsKey(operation.Id))
            return new ValueTask<bool>(false);

        Operations[operation.Id].Status = newStatus;

        return new ValueTask<bool>(true);
    }

    public override ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task,
        object? data, IDictionary<string,string>? messageHeaders)
    {
        if(!Progress.ContainsKey(progressId))
            Progress.Add(operation.Id, new List<object>());
     
        if(data!= null)
            Progress[operation.Id].Add(data);
        return ValueTask.CompletedTask;
    }
}