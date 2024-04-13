using System;

namespace Dbosoft.Rebus.Operations.Events;

public class OperationTaskProgressEvent
{
    public Guid Id { get; set; }

    public Guid OperationId { get; set; }
    public Guid TaskId { get; set; }
    public object? Data { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}