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
        return await _repository.GetByIdAsync(taskId);
    }

    public async ValueTask<IOperationTask> GetOrCreateAsync(IOperation operation, object command, Guid taskId, Guid parentTaskId)
    {
        var model = await _repository.GetByIdAsync(taskId);
        if (model != null)
            return model;
        
        model = new OperationTaskModel
        {
            Id = taskId,
            Created = DateTimeOffset.Now,
            LastUpdate = DateTimeOffset.Now,
            InitiatingTaskId = parentTaskId,
            OperationId = operation.Id,
            Status = OperationTaskStatus.Queued
        };

        await _repository.AddAsync(model);
        return model;
    }

    public async ValueTask<bool> TryChangeStatusAsync(IOperationTask task, OperationTaskStatus newStatus, object? additionalData)
    {
        var model = await _repository.GetByIdAsync(task.Id);
        if (model == null)
            return false;
        
        model.Status = newStatus;

        return true;
    }
}