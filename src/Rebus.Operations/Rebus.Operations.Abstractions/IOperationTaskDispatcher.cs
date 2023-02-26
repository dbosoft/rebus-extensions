#nullable enable

using System;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations
{
    public interface IOperationTaskDispatcher
    {
        ValueTask<IOperationTask?> StartNew<T>(Guid operationId, Guid initiatingTaskId,
            object? additionalData = default) where T : class, new();

        ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, Type operationCommandType,
            object? additionalData = default);

        ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, object command
            , object? additionalData = default);

    }

}