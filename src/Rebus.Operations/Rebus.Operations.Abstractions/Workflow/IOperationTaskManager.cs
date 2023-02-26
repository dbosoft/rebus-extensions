using System;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Workflow;

#nullable enable

public interface IOperationTaskManager
{
    ValueTask<IOperationTask?> GetByIdAsync(Guid taskId);
    ValueTask<IOperationTask> GetOrCreateAsync(IOperation operation, object command, Guid taskId, Guid parentTaskId);
    ValueTask<bool> TryChangeStatusAsync(IOperationTask task, OperationTaskStatus newStatus, object? additionalData);


}