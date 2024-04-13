using System;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Workflow;

public interface IOperationTaskManager
{
    ValueTask<IOperationTask?> GetByIdAsync(Guid taskId);
    ValueTask<IOperationTask> GetOrCreateAsync(IOperation operation, object command,
        DateTimeOffset created,
        Guid taskId, Guid parentTaskId);
    ValueTask<bool> TryChangeStatusAsync(IOperationTask task, OperationTaskStatus newStatus, 
        DateTimeOffset timestamp,
        object? additionalData);


}