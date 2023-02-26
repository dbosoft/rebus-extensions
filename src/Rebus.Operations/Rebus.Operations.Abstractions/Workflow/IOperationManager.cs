using System;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Workflow;

#nullable enable
public interface IOperationManager
{
    ValueTask<IOperation?> GetByIdAsync(Guid operationId);
    ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command);

    ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus, object? additionalData);
    ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task, object? data);
}