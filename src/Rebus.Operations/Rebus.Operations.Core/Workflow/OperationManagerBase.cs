#nullable enable
using System;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Workflow;

public abstract class OperationManagerBase : IOperationManager
{

    public abstract ValueTask<IOperation?> GetByIdAsync(Guid operationId);

    public abstract ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command);


    public abstract ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation,
        IOperationTask task,
        object? data);

    public abstract ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus,
        object? additionalData);





}