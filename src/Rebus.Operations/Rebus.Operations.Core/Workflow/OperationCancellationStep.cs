using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Dbosoft.Rebus.Operations.Workflow;

/// <summary>
/// Incoming pipeline step that turns a cancelled task handler into a reported
/// cancellation. When a task handler observes its <see cref="System.Threading.CancellationToken"/>
/// and throws <see cref="OperationCanceledException"/>, this step reports the task as
/// <see cref="OperationTaskStatus.Cancelled"/> and swallows the exception so Rebus does
/// not retry it. Any other exception (or a cancellation not tied to the task's token)
/// is left to propagate.
/// </summary>
[StepDocumentation("Reports tasks that were cancelled via their cancellation token and stops them from being retried.")]
public class OperationCancellationStep : IIncomingStep
{
    private readonly Func<IBus> _bus;
    private readonly WorkflowOptions _options;
    private readonly ITaskCancellationRegistry _cancellationRegistry;
    private readonly ILogger _logger;

    public OperationCancellationStep(
        Func<IBus> bus,
        WorkflowOptions options,
        ITaskCancellationRegistry cancellationRegistry,
        ILogger logger)
    {
        _bus = bus;
        _options = options;
        _cancellationRegistry = cancellationRegistry;
        _logger = logger;
    }

    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        // Resolve the task message up front so both the cancellation and the cleanup
        // paths can use it.
        IOperationTaskMessage? taskMessage = null;
        if (context.Load<Message>()?.Body is { } body)
            IsTaskExecutionMessage(body, out taskMessage);

        try
        {
            await next().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
            // Only treat this as a cancellation when the message being handled is the
            // task-execution message AND this registry actually tripped the task's token.
            // Consulting the registry (rather than the exception's token) ensures an
            // unrelated OperationCanceledException — e.g. from a handler's own timeout
            // source or a host-shutdown token — is left to propagate (fail/retry).
            when (taskMessage is not null
                  && _cancellationRegistry.IsCancellationRequested(taskMessage.OperationId, taskMessage.TaskId))
        {
            _logger.LogDebug("Operation Workflow {operationId}, Task {taskId}: handler cancelled, reporting task as cancelled",
                taskMessage.OperationId, taskMessage.TaskId);

            // Report first, then drop the registration. If the send fails the message is
            // retried with the registration still in place, so the retried handler observes
            // the (still cancelled) token rather than registering a fresh, non-cancelled one.
            await _bus().SendWorkflowEvent(_options,
                OperationTaskStatusEvent.Cancelled(
                    taskMessage.OperationId, taskMessage.InitiatingTaskId, taskMessage.TaskId))
                .ConfigureAwait(false);
            _cancellationRegistry.Remove(taskMessage.OperationId, taskMessage.TaskId);
        }
        catch
        {
            // Any other failure (including an unrelated OperationCanceledException): drop the
            // opt-in registration so a handler that fails outside the terminal ITaskMessaging
            // paths does not leak its CancellationTokenSource, then let the failure propagate.
            if (taskMessage is not null)
                _cancellationRegistry.Remove(taskMessage.OperationId, taskMessage.TaskId);
            throw;
        }
    }

    private static bool IsTaskExecutionMessage(object body, [MaybeNullWhen(false)] out IOperationTaskMessage taskMessage)
    {
        // The task-execution message is OperationTask<T>; other IOperationTaskMessage
        // types (status events etc.) are not handled by cancellable task handlers.
        if (body is IOperationTaskMessage message
            && body.GetType() is { IsGenericType: true } type
            && type.GetGenericTypeDefinition() == typeof(OperationTask<>))
        {
            taskMessage = message;
            return true;
        }

        taskMessage = null;
        return false;
    }
}
