using System;

namespace Dbosoft.Rebus.Operations.Events;

/// <summary>
/// Broadcast by <c>IOperationDispatcher.RequestCancellation</c> when cancellation
/// of an operation has been requested. Worker hosts subscribe to this event and
/// cancel the <see cref="System.Threading.CancellationToken"/> of any of the
/// operation's tasks that are currently running locally.
/// </summary>
public class OperationCancellationRequestedEvent
{
    public Guid OperationId { get; set; }
}
