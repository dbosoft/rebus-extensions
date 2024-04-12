using System;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Workflow;

#nullable enable
public abstract class OperationTaskManagerBase : IOperationTaskManager
{

    public abstract ValueTask<IOperationTask?> GetByIdAsync(Guid taskId);

    public abstract ValueTask<IOperationTask> GetOrCreateAsync(IOperation operation, object command,
        DateTimeOffset created, Guid taskId, Guid parentTaskId);

    public abstract ValueTask<bool> TryChangeStatusAsync(IOperationTask task, 
        OperationTaskStatus newStatus,
        DateTimeOffset timestamp,
        object? additionalData);


}