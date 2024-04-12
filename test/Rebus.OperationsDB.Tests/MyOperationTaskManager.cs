using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Workflow;
using Dbosoft.Rebus.OperationsDB.Tests.Models;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class MyOperationTaskManager : IOperationTaskManager
{
    private readonly IStateStoreRepository<OperationTaskModel> _repository;
  
    public MyOperationTaskManager(IStateStoreRepository<OperationTaskModel> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IOperationTask?> GetByIdAsync(Guid taskId)
    {
        return await _repository.GetByIdAsync(taskId).ConfigureAwait(false);
    }

    public async ValueTask<IOperationTask> GetOrCreateAsync(IOperation operation, 
        object command,
        DateTimeOffset timestamp,
        Guid taskId, Guid parentTaskId)
    {
        var model = await _repository.GetByIdAsync(taskId).ConfigureAwait(false);
        if (model != null)
            return model;
        
        model = new OperationTaskModel
        {
            Id = taskId,
            Created = timestamp,
            LastUpdate = timestamp,
            InitiatingTaskId = parentTaskId,
            OperationId = operation.Id,
            Status = OperationTaskStatus.Queued
        };

        await _repository.AddAsync(model).ConfigureAwait(false);
        return model;
    }

    public async ValueTask<bool> TryChangeStatusAsync(IOperationTask task, 
        OperationTaskStatus newStatus, 
        DateTimeOffset timestamp,
        object? additionalData)
    {
        var model = await _repository.GetByIdAsync(task.Id).ConfigureAwait(false);
        if (model == null)
            return false;

        if (model.LastUpdate > timestamp)
            return false;

        model.Status = newStatus;
        model.LastUpdate = timestamp;
        return true;
    }
}