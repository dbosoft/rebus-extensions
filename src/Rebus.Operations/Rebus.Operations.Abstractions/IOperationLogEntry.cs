using System;

namespace Dbosoft.Rebus.Operations;

public interface IOperationLogEntry
{
    string Message { get; }
    public DateTimeOffset Timestamp { get; }

}