using System;

namespace Dbosoft.Rebus.Operations.Events;

public class OperationStatusEvent
{
    public Guid OperationId { get; set; }
    public OperationStatus NewStatus { get; set; }
}