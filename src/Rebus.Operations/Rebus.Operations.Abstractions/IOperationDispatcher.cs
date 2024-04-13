using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations;

[PublicAPI]
public interface IOperationDispatcher
{
    ValueTask<IOperation?> StartNew<T>(object? additionalData = default, IDictionary<string,string>? additionalHeaders = null) where T : class, new();
    ValueTask<IOperation?> StartNew(Type commandType, object? additionalData = default, IDictionary<string,string>? additionalHeaders = null);
    ValueTask<IOperation?> StartNew(object operationCommand,  object? additionalData = default, IDictionary<string,string>? additionalHeaders = null);
  
}