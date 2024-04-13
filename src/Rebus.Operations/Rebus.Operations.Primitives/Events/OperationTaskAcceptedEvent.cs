using System;

namespace Dbosoft.Rebus.Operations.Events;

public class OperationTaskAcceptedEvent : IOperationTaskMessage
{
    public Guid OperationId { get; set; }
    public Guid InitiatingTaskId { get; set; }
    public Guid TaskId { get; set; }

    public object? AdditionalData { get; set; }
    public DateTimeOffset Created { get; set; }
}