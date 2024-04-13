using JetBrains.Annotations;
using System;

namespace Dbosoft.Rebus.Operations;

[PublicAPI]
public interface IOperationLogEntry
{
    string Message { get; }
    public DateTimeOffset Timestamp { get; }

}