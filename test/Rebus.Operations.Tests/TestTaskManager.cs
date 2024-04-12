using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestTaskManager : OperationTaskManagerBase
{
    public static readonly Dictionary<Guid, TestOperationTaskModel> Tasks = new();

    public static void Reset()
    {
        Tasks.Clear();
    }
    
    public override ValueTask<IOperationTask?> GetByIdAsync(Guid taskId)
    {
        return ValueTask.FromResult( 
            Tasks.ContainsKey(taskId) 
                ? Tasks[taskId] as IOperationTask : null);

    }

    public override ValueTask<IOperationTask> GetOrCreateAsync(IOperation operation, 
        object command, DateTimeOffset created, Guid taskId, Guid parentTaskId)
    {
        if (Tasks.ContainsKey(taskId))
            return GetByIdAsync(taskId)!;
        
        var task = new TestOperationTaskModel
        {
            Id = taskId,
            OperationId = operation.Id,
            InitiatingTaskId = parentTaskId,
            Status = OperationTaskStatus.Queued
        };
        Tasks.Add(taskId, task);
        return new ValueTask<IOperationTask>(task);
    }

    public override ValueTask<bool> TryChangeStatusAsync(IOperationTask task, OperationTaskStatus newStatus, DateTimeOffset created, object? additionalData)
    {
        if (!Tasks.ContainsKey(task.Id))
            return new ValueTask<bool>(false);

        Tasks[task.Id].Status = newStatus;

        return new ValueTask<bool>(true);
    }
}