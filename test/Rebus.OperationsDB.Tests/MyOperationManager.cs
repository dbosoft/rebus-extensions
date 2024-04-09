using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Workflow;
using Dbosoft.Rebus.OperationsDB.Tests.Models;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class MyOperationManager : IOperationManager
{
    private readonly IStateStoreRepository<OperationModel> _repository;
    private readonly IStateStoreRepository<OperationLogEntry> _logRepository;
    private readonly IStateStoreRepository<OperationTaskModel> _taskRepository;

    public MyOperationManager(IStateStoreRepository<OperationModel> repository, 
        IStateStoreRepository<OperationLogEntry> logRepository, IStateStoreRepository<OperationTaskModel> taskRepository)
    {
        _repository = repository;
        _logRepository = logRepository;
        _taskRepository = taskRepository;
    }

    public async ValueTask<IOperation?> GetByIdAsync(Guid operationId)
    {
        return await _repository.GetByIdAsync(operationId);
    }

    public async ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command, object? additionalData, IDictionary<string, string>? additionalHeaders)
    {
        var model = await _repository.GetByIdAsync(operationId);
        if (model != null)
            return model;
        
        model = new OperationModel
        {
            Id = operationId,
            Created = DateTimeOffset.Now,
            LastUpdate = DateTimeOffset.Now,
            Status = OperationStatus.Queued,
        };

        await _repository.AddAsync(model);
        return model;

    }

    public async ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus, object? additionalData,
        IDictionary<string, string>? messageHeaders)
    {
        var model = await _repository.GetByIdAsync(operation.Id);
        if (model == null)
            return false;
        
        model.Status = newStatus;
        model.LastUpdate = DateTimeOffset.Now;
        return true;
    }

    public async ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task,
        object? data, IDictionary<string, string>? messageHeaders)
    {
        var message = "";
        var progress = 0;

        if (data is string msgString)
            message = msgString;
        
        if (data is int progressMsg)
            progress = progressMsg;

        var taskEntry = await _taskRepository.GetByIdAsync(task.Id);
        if (taskEntry != null)
        {
            taskEntry.Progress = progress;
            taskEntry.LastUpdate = DateTimeOffset.Now;
        }

        var opLogEntry =
            new OperationLogEntry
            {
                Id = progressId,
                OperationId = operation.Id,
                TaskId = task.Id,
                Message = message,
                Timestamp = timestamp
            };

        await _logRepository.AddAsync(opLogEntry).ConfigureAwait(false);


    }
}