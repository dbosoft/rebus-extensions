using System;

namespace Dbosoft.Rebus.Operations.Events;

public class OperationTimeoutEvent
{
    public Guid OperationId { get; set; }
}

public class OperationCompleteEvent
{
    public Guid OperationId { get; set; }
}