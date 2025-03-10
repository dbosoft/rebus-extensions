using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations;

[PublicAPI]
public interface IOperationDispatcher
{
    ValueTask<IOperation> StartNew<TCommand>(
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null)
        where TCommand : class, new();

    ValueTask<IOperation> StartNew(
        Type commandType,
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null);

    ValueTask<IOperation> StartNew(
        object command,
        object? additionalData = null,
        IDictionary<string,string>? additionalHeaders = null);
}
