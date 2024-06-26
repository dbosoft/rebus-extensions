﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Workflow;

public interface IOperationManager
{
    ValueTask<IOperation?> GetByIdAsync(Guid operationId);
    ValueTask<IOperation> GetOrCreateAsync(Guid operationId, object command, 
        DateTimeOffset timestamp,
        object? additionalData, IDictionary<string,string>? additionalHeaders);

    ValueTask<bool> TryChangeStatusAsync(IOperation operation, OperationStatus newStatus, 
        DateTimeOffset timestamp, object? additionalData, 
        IDictionary<string,string>? messageHeaders);
    ValueTask AddProgressAsync(Guid progressId, DateTimeOffset timestamp, IOperation operation, IOperationTask task, 
        object? data, IDictionary<string,string>? messageHeaders);
}