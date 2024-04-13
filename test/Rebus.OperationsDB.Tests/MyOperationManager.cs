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
        return await _repository.GetByIdAsync(operationId).ConfigureAwait(false);
    }

    public async ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command,
        DateTimeOffset timestamp, object? additionalData, IDictionary<string, string>? additionalHeaders)
    {
        var model = await _repository.GetByIdAsync(operationId).ConfigureAwait(false);
        if (model != null)
            return model;
        
        model = new OperationModel
        {
            Id = operationId,
            Created = timestamp,
            LastUpdate = timestamp,
            Status = OperationStatus.Queued
        };

        await _repository.AddAsync(model).ConfigureAwait(false);
        return model;

    }

    public async ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus, 
        DateTimeOffset timestamp,
        object? additionalData,
        IDictionary<string, string>? messageHeaders)
    {
        var model = await _repository.GetByIdAsync(operation.Id).ConfigureAwait(false);
        if (model == null)
            return false;

        if (model.LastUpdate > timestamp)
            return false;

        model.Status = newStatus;
        model.LastUpdate = timestamp;
        return true;
    }

    public async ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task,
        object? data, IDictionary<string, string>? messageHeaders)
    {
        var message = "";
        var progress = 0;

        switch (data)
        {
            case string msgString:
                message = msgString;
                break;
            case int progressMsg:
                progress = progressMsg;
                break;
        }

        var taskEntry = await _taskRepository.GetByIdAsync(task.Id).ConfigureAwait(false);
        if (taskEntry != null)
        {
            taskEntry.Progress = progress;
            taskEntry.LastUpdate = timestamp;
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