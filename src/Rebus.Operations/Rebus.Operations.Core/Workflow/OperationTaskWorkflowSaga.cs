#nullable enable

using System;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Workflow
{
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

        protected Task Fail(object? message = null)
        {
            return WorkflowEngine.Messaging.DispatchTaskStatusEventAsync(OperationTaskStatusEvent.Failed(
                Data.OperationId, Data.ParentTaskId, Data.SagaTaskId, message, 
                WorkflowEngine.WorkflowOptions.JsonSerializerOptions));
        }


        protected Task Complete(object? message = null)
        {
            return WorkflowEngine.Messaging.DispatchTaskStatusEventAsync(OperationTaskStatusEvent.Completed(
                Data.OperationId, Data.ParentTaskId, Data.SagaTaskId, message,
                WorkflowEngine.WorkflowOptions.JsonSerializerOptions));
        }

        protected async Task FailOrRun<T>(OperationTaskStatusEvent<T> message, Func<Task> completedFunc)
            where T : class, new()
        {
            if (message.InitiatingTaskId != Data.SagaTaskId)
                return;
            
            if (message.OperationFailed)
                await Fail(message.GetMessage(WorkflowEngine.WorkflowOptions.JsonSerializerOptions));

            await completedFunc();
        }

        protected async Task FailOrRun<T, TOpMessage>(OperationTaskStatusEvent<T> message, Func<TOpMessage, Task> completedFunc)
            where T : class, new()
            where TOpMessage : class
        {
            if (message.InitiatingTaskId != Data.SagaTaskId)
                return;

            if (message.OperationFailed)
                await Fail(message.GetMessage(WorkflowEngine.WorkflowOptions.JsonSerializerOptions));
            else
                await completedFunc(
                    message.GetMessage(WorkflowEngine.WorkflowOptions.JsonSerializerOptions) as TOpMessage
                    ?? throw new InvalidOperationException(
                        $"Message {typeof(T)} has not returned a result of type {typeof(TOpMessage)}."));
        }


        protected ValueTask<IOperationTask?> StartNewTask<T>(object? additionalData = default) where T : class, new()
        {

            return WorkflowEngine.Messaging.TaskDispatcher.StartNew<T>(Data.OperationId, Data.SagaTaskId, additionalData);
        }


        protected ValueTask<IOperationTask?> StartNewTask(Type taskCommandType,
            object? additionalData = default)
        {
            return WorkflowEngine.Messaging.TaskDispatcher.StartNew(Data.OperationId, Data.SagaTaskId, taskCommandType, additionalData);

        }

  
        protected ValueTask<IOperationTask?> StartNewTask(object command, object? additionalData = default)
        {
            return WorkflowEngine.Messaging.TaskDispatcher.StartNew(Data.OperationId, Data.SagaTaskId, command, additionalData);

        }



    }
}