using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Workflow;
using Dbosoft.Rebus.OperationsDB.Tests.Models;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class MyOperationManager : IOperationManager
{
    private readonly IStateStoreRepository<OperationModel> _repository;

    public MyOperationManager(IStateStoreRepository<OperationModel> repository)
    {
        _repository = repository;
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
            Status = OperationStatus.Queued
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

        return true;
    }

    public ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task,
        object? data, IDictionary<string, string>? messageHeaders)
    {
        return ValueTask.CompletedTask;
    }
}