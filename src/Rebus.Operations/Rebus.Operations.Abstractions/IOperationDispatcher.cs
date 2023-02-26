#nullable enable

using System;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations
{
    public interface IOperationDispatcher
    {
        ValueTask<IOperation?> StartNew<T>(object? additionalData = default) where T : class, new();
        ValueTask<IOperation?> StartNew(Type commandType, object? additionalData = default);
        ValueTask<IOperation?> StartNew(object operationCommand);

    }
}