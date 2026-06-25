using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Workflow;

/// <summary>
/// Worker-side handler for the broadcast <see cref="OperationCancellationRequestedEvent"/>.
/// Trips the cancellation tokens of the operation's tasks that are running in
/// this process. Tasks that did not opt in to cancellation are unaffected.
/// </summary>
public class OperationCancellationRequestedHandler : IHandleMessages<OperationCancellationRequestedEvent>
{
    private readonly ITaskCancellationRegistry _cancellationRegistry;
    private readonly ILogger<OperationCancellationRequestedHandler> _logger;

    public OperationCancellationRequestedHandler(
        ITaskCancellationRegistry cancellationRegistry,
        ILogger<OperationCancellationRequestedHandler> logger)
    {
        _cancellationRegistry = cancellationRegistry;
        _logger = logger;
    }

    public Task Handle(OperationCancellationRequestedEvent message)
    {
        _logger.LogDebug("Cancellation requested for operation {operationId}", message.OperationId);
        _cancellationRegistry.Cancel(message.OperationId);
        return Task.CompletedTask;
    }
}
