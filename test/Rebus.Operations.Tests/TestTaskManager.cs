using System.Collections.Concurrent;
using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestTaskManager(TestOperationStore store) : OperationTaskManagerBase
{
    public override ValueTask<IOperationTask?> GetByIdAsync(Guid taskId)
    {
        store.Tasks.TryGetValue(taskId, out var task);
        return ValueTask.FromResult<IOperationTask?>(task);
    }

    public override ValueTask<IOperationTask> GetOrCreateAsync(
        IOperation operation, 
        object command,
        DateTimeOffset created,
        Guid taskId,
        Guid parentTaskId)
    {
        var task = store.Tasks.GetOrAdd(
            taskId,
            new TestOperationTaskModel
            {
                Id = taskId,
                OperationId = operation.Id,
                InitiatingTaskId = parentTaskId,
                Status = OperationTaskStatus.Queued
            });

        if (task.OperationId != operation.Id)
            throw new InvalidOperationException("Task already exists for another operation");

        if (task.InitiatingTaskId != parentTaskId)
            throw new InvalidOperationException("Task already exists with a different parent");
        
        return new ValueTask<IOperationTask>(task);
    }

    public override ValueTask<bool> TryChangeStatusAsync(
        IOperationTask task,
        OperationTaskStatus newStatus,
        DateTimeOffset timestamp,
        object? additionalData)
    {
        if (!store.Tasks.TryGetValue(task.Id, out var taskModel))
            return ValueTask.FromResult(false);

        lock (taskModel)
        {
            taskModel.Status = newStatus;
        }

        return ValueTask.FromResult(true);
    }
}
