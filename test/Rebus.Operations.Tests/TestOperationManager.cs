using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestOperationManager : OperationManagerBase
{
    public readonly Dictionary<Guid, TestOperationModel> Operations = new();

    public override ValueTask<IOperation?> GetByIdAsync(Guid operationId)
    {
        return ValueTask.FromResult( 
            Operations.ContainsKey(operationId) 
                ? Operations[operationId] as IOperation : null);

    }

    public override ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command)
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

    public override ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus, object? additionalData)
    {
        if (!Operations.ContainsKey(operation.Id))
            return new ValueTask<bool>(false);

        Operations[operation.Id].Status = newStatus;

        return new ValueTask<bool>(true);
    }

    public override ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task,
        object? data)
    {
        return ValueTask.CompletedTask;
    }
}