#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations
{
    public interface IOperationTaskDispatcher
    {
        ValueTask<IOperationTask?> StartNew<T>(Guid operationId, Guid initiatingTaskId,
            object? additionalData = default, IDictionary<string,string>? additionalHeaders = null) where T : class, new();

        ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, Type operationCommandType,
            object? additionalData = default, IDictionary<string,string>? additionalHeaders = null);

        ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, object command
            , object? additionalData = default, IDictionary<string,string>? additionalHeaders = null);

    }

}