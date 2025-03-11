using System;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using JetBrains.Annotations;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Workflow;

[PublicAPI]
public abstract class OperationTaskWorkflowSaga<TMessage, TSagaData> : Saga<TSagaData>,
    IAmInitiatedBy<OperationTask<TMessage>>,
    IHandleMessages<OperationTaskStatusEvent<TMessage>>
    where TSagaData : TaskWorkflowSagaData, new()
    where TMessage : class, new()
{
    protected readonly IWorkflow WorkflowEngine;

    protected OperationTaskWorkflowSaga(IWorkflow workflowEngine)
    {
        WorkflowEngine = workflowEngine;
    }


    public virtual Task Handle(OperationTask<TMessage> message)
    {
        Data.OperationId = message.OperationId;
        Data.SagaTaskId = message.TaskId;
        Data.ParentTaskId = message.InitiatingTaskId;
        return Initiated(message.Command);
    }

    public Task Handle(OperationTaskStatusEvent<TMessage> message)
    {
        return message.OperationFailed ? InitiatingTaskFailed() : InitiatingTaskCompleted();
    }

    protected override void CorrelateMessages(ICorrelationConfig<TSagaData> config)
    {
        config.Correlate<OperationTask<TMessage>>(m => m.TaskId, d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<TMessage>>(m => m.TaskId, d => d.SagaTaskId);
    }

    protected abstract Task Initiated(TMessage message);

    private Task InitiatingTaskCompleted()
    {
        MarkAsComplete();
        return Task.CompletedTask;
    }

    private Task InitiatingTaskFailed()
    {
        MarkAsComplete();
        return Task.CompletedTask;
    }

    protected Task Fail(string errorMessage)
    {
        return WorkflowEngine.Messaging.DispatchTaskStatusEventAsync(OperationTaskStatusEvent.Failed(
            Data.OperationId, Data.ParentTaskId, Data.SagaTaskId,
            new ErrorData { ErrorMessage = errorMessage },
            WorkflowEngine.WorkflowOptions.JsonSerializerOptions));
    }

    protected Task Fail(ErrorData error)
    {
        return WorkflowEngine.Messaging.DispatchTaskStatusEventAsync(OperationTaskStatusEvent.Failed(
            Data.OperationId, Data.ParentTaskId, Data.SagaTaskId, error,
            WorkflowEngine.WorkflowOptions.JsonSerializerOptions));
    }

    protected Task Complete()
    {
        return WorkflowEngine.Messaging.DispatchTaskStatusEventAsync(OperationTaskStatusEvent.Completed(
            Data.OperationId, Data.ParentTaskId, Data.SagaTaskId));
    }

    protected Task Complete(object message)
    {
        return WorkflowEngine.Messaging.DispatchTaskStatusEventAsync(OperationTaskStatusEvent.Completed(
            Data.OperationId, Data.ParentTaskId, Data.SagaTaskId, message,
            WorkflowEngine.WorkflowOptions.JsonSerializerOptions));
    }

    protected async Task FailOrRun<TCommand>(
        OperationTaskStatusEvent<TCommand> message,
        Func<Task> completedFunc)
        where TCommand : class, new()
    {
        if (message.InitiatingTaskId != Data.SagaTaskId)
            return;

        if (message.OperationFailed)
        {
            await Fail(message).ConfigureAwait(false);
            return;
        }

        await completedFunc().ConfigureAwait(false);
    }

    protected async Task FailOrRun<TCommand, TCommandResponse>(
        OperationTaskStatusEvent<TCommand> message,
        Func<TCommandResponse, Task> completedFunc)
        where TCommand : class, new()
        where TCommandResponse : class
    {
        if (message.InitiatingTaskId != Data.SagaTaskId)
            return;

        if (message.OperationFailed)
        {
            await Fail(message).ConfigureAwait(false);
            return;
        }

        if (message.GetMessage(WorkflowEngine.WorkflowOptions.JsonSerializerOptions) is not TCommandResponse response)
            throw new InvalidOperationException($"Message {typeof(TCommand)} has not returned a result of type {typeof(TCommandResponse)}.");

        await completedFunc(response).ConfigureAwait(false);
    }

    private async Task Fail<TCommand>(
        OperationTaskStatusEvent<TCommand> message)
        where TCommand : class, new()
    {
        if (!message.OperationFailed)
            throw new ArgumentException("The operation has not failed.", nameof(message));

        var messageData = message.GetMessage(WorkflowEngine.WorkflowOptions.JsonSerializerOptions);
        if (messageData is ErrorData errorData)
        {
            await Fail(errorData).ConfigureAwait(false);
            return;
        }

        if (messageData is null)
        {
            await Fail($"The task {message.TaskId} of the saga {Data.SagaTaskId} failed without any error data.")
                .ConfigureAwait(false);
            return;
        }

        throw new InvalidOperationException(
            $"The failed task {message.TaskId} of the saga {Data.SagaTaskId} should contain {nameof(ErrorData)} but contains data of type {messageData.GetType().Name}.");
    }

    protected ValueTask<IOperationTask> StartNewTask<TCommand>(
        object? additionalData = null)
        where TCommand : class, new()
    {
        return WorkflowEngine.Messaging.TaskDispatcher.StartNew<TCommand>(
            Data.OperationId, Data.SagaTaskId, additionalData);
    }

    protected ValueTask<IOperationTask> StartNewTask(
        Type commandType,
        object? additionalData = null)
    {
        return WorkflowEngine.Messaging.TaskDispatcher.StartNew(
            Data.OperationId, Data.SagaTaskId, commandType, additionalData);
    }

    protected ValueTask<IOperationTask> StartNewTask(
        object command,
        object? additionalData = null)
    {
        return WorkflowEngine.Messaging.TaskDispatcher.StartNew(
            Data.OperationId, Data.SagaTaskId, command, additionalData);
    }
}
