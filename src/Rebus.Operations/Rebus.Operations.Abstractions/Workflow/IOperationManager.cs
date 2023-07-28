using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Workflow;

#nullable enable
public interface IOperationManager
{
    ValueTask<IOperation?> GetByIdAsync(Guid operationId);
    ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command, 
        object? additionalData, IDictionary<string,string>? additionalHeaders);

    ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus, object? additionalData, 
        IDictionary<string,string>? messageHeaders);
    ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task, 
        object? data, IDictionary<string,string>? messageHeaders);
}