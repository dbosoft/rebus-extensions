using System.Collections.Concurrent;
using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestOperationManager(TestOperationStore store): OperationManagerBase
{
    public override ValueTask<IOperation?> GetByIdAsync(Guid operationId)
    {
        store.Operations.TryGetValue(operationId, out var operation);
        return ValueTask.FromResult<IOperation?>(operation);
    }

    public override ValueTask<IOperation> GetOrCreateAsync(
        Guid operationId,
        object command,
        DateTimeOffset timestamp,
        object? additionalData,
        IDictionary<string,string>? additionalHeaders)
    {
        var operation = store.Operations.GetOrAdd(
            operationId,
            id => new TestOperationModel()
            {
                Id = id,
            });
        
        return ValueTask.FromResult<IOperation>(operation);
    }

    public override ValueTask<bool> TryChangeStatusAsync(
        IOperation operation, 
        OperationStatus newStatus, 
        DateTimeOffset timestamp,
        object? additionalData,
        IDictionary<string,string>? messageHeaders)
    {
        if (!store.Operations.TryGetValue(operation.Id, out var operationModel))
            return ValueTask.FromResult(false);

        lock (operationModel)
        {
            operationModel.Status = newStatus;
        }

        return ValueTask.FromResult(true);
    }

    public override ValueTask AddProgressAsync(
        Guid progressId,
        DateTimeOffset timestamp,
        IOperation operation,
        IOperationTask task,
        object? data,
        IDictionary<string,string>? messageHeaders)
    {
        var progress = store.Progress.GetOrAdd(
            progressId,
            _ => new ConcurrentQueue<object?>());

        progress.Enqueue(data);

        return ValueTask.CompletedTask;
    }
}
