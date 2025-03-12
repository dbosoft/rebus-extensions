using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations;

public interface IOperationTaskDispatcher
{
    ValueTask<IOperationTask> StartNew<TCommand>(
        Guid operationId,
        Guid initiatingTaskId,
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null)
        where TCommand : class, new();

    ValueTask<IOperationTask> StartNew(
        Guid operationId,
        Guid initiatingTaskId,
        Type commandType,
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null);

    ValueTask<IOperationTask> StartNew(
        Guid operationId,
        Guid initiatingTaskId,
        object command,
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null);
}
