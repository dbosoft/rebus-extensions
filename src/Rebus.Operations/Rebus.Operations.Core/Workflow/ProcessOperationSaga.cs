#nullable enable

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Pipeline;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Operations.Workflow
{
    [UsedImplicitly]
    public class ProcessOperationSaga : Saga<OperationSagaData>,
        IAmInitiatedBy<CreateOperationCommand>,
        IHandleMessages<CreateNewOperationTaskCommand>,
        IHandleMessages<OperationTaskAcceptedEvent>,
        IHandleMessages<OperationTaskStatusEvent>,
        IHandleMessages<OperationTimeoutEvent>
    {
        private readonly IWorkflow _workflow;
        private readonly ILogger _log;

        public ProcessOperationSaga(IWorkflow workflow, ILogger log)
        {
            _workflow = workflow;
            _log = log;
        }

        public Task Handle(CreateOperationCommand message)
        {
            if(message.TaskMessage == null)
                throw new InvalidOperationException($"Operation Workflow: invalid command - missing task message");
            
            Data.OperationId = message.TaskMessage.OperationId;
            Data.PrimaryTaskId = message.TaskMessage.TaskId;
            
            return Handle(message.TaskMessage);
        }

        public Task Handle(OperationTimeoutEvent? message)
        {
            return Task.CompletedTask;
        }

        protected override void CorrelateMessages(ICorrelationConfig<OperationSagaData> config)
        {
            config.Correlate<CreateOperationCommand>(m => m.TaskMessage?.OperationId, d => d.OperationId);
            config.Correlate<CreateNewOperationTaskCommand>(m => m.OperationId, d => d.OperationId);
            config.Correlate<OperationTimeoutEvent>(m => m.OperationId, d => d.OperationId);
            config.Correlate<OperationTaskAcceptedEvent>(m => m.OperationId, d => d.OperationId);
            config.Correlate<OperationTaskStatusEvent>(m => m.OperationId, d => d.OperationId);
        }

        public async Task Handle(CreateNewOperationTaskCommand message)
        {
            if(string.IsNullOrWhiteSpace(message.CommandData))
                throw new InvalidOperationException($"Operation Workflow {message.OperationId}: missing command data");
            if(string.IsNullOrWhiteSpace(message.CommandType))
                throw new InvalidOperationException($"Operation Workflow {message.OperationId}: missing command type");

            var command = JsonSerializer.Deserialize(message.CommandData,
                Type.GetType(message.CommandType) ??
                throw new InvalidOperationException($"Operation Workflow {message.OperationId}: unknown command type '{message.CommandType}'"),
                _workflow.WorkflowOptions.JsonSerializerOptions);
            
            if(command == null)
                throw new InvalidOperationException($"Operation Workflow {message.OperationId}: invalid command data in message '{message.CommandType}'");

            _log.LogDebug("Operation Workflow {operationId}, Task {taskId}: creating new task command '{commandType}'",
                message.OperationId, message.TaskId, command.GetType());


            var op = await _workflow.Operations
                .GetByIdAsync(message.OperationId)
                .ConfigureAwait(false);


            if (op == null)
            {
                _log.LogWarning("Operation Workflow {operationId}: Operation not found - cancelling workflow",
                    message.OperationId);
                MarkAsComplete();
                return;
            }

            var task = await _workflow.Tasks
                .GetOrCreateAsync(op, command, message.TaskId, message.InitiatingTaskId)
                .ConfigureAwait(false);


            var messageType = Type.GetType(message.CommandType);
            if (messageType == null)
                throw new InvalidOperationException($"unknown command type '{message.CommandType}'");

            Data.Tasks.Add(message.TaskId, messageType.AssemblyQualifiedName!);
            await _workflow.Messaging.DispatchTaskMessage(command,task);
        }


        public async Task Handle(OperationTaskAcceptedEvent message)
        {
            var op = await _workflow.Operations
                .GetByIdAsync(message.OperationId)
                .ConfigureAwait(false);

            var task = await _workflow.Tasks
                .GetByIdAsync(message.TaskId)
                .ConfigureAwait(false);

            if (op == null || task == null)
            {
                _log.LogDebug("Operation Workflow {operationId}, Task {taskId}: could not accept task as it was not found.", 
                    message.OperationId, message.TaskId);
                return;
            }

            var opOldStatus = op.Status;
            if (await _workflow.Operations.TryChangeStatusAsync(op, OperationStatus.Running, null, 
                    MessageContext.Current.Headers))
            {
                _log.LogDebug("Operation Workflow {operationId}: Status changed: {oldStatus} -> {newStatus}",
                    message.OperationId, opOldStatus, op.Status);

                
                await _workflow.Messaging.DispatchOperationStatusEventAsync(new OperationStatusEvent
                {
                    OperationId = op.Id,
                    NewStatus = OperationStatus.Running
                });


                var taskOldStatus = task.Status;
                if (await _workflow.Tasks.TryChangeStatusAsync(task, OperationTaskStatus.Running,
                        message.AdditionalData))
                {
                    _log.LogDebug("Operation Workflow {operationId}, Task {taskId}: Status changed: {oldStatus} -> {newStatus}",
                        message.OperationId, message.TaskId, taskOldStatus, task.Status);

                }
            }

        }

        public async Task Handle(OperationTaskStatusEvent message)
        {
            var op = await _workflow.Operations
                .GetByIdAsync(message.OperationId)
                .ConfigureAwait(false);

            var task = await _workflow.Tasks
                .GetByIdAsync(message.TaskId)
                .ConfigureAwait(false);

            if (op == null || task == null)
            {
                _log.LogDebug("Operation Workflow {operationId}, Task {taskId}: could not update task status as it was not found",
                    message.OperationId, message.TaskId);
                return;
            }

            if (task.Status is OperationTaskStatus.Queued or OperationTaskStatus.Running)
            {
                if(!Data.Tasks.ContainsKey(message.TaskId))
                    _log.LogWarning("Operation Workflow {operationId}, Task {taskId}: could not update task status as it was not found in workflow.",
                        message.OperationId, message.TaskId);
                else
                {
                    var taskCommandTypeName = Data.Tasks[message.TaskId];
                    await _workflow.Messaging.DispatchTaskStatusEventAsync(taskCommandTypeName, message);
                }
            }

            var taskOldStatus = task.Status;
            if(await _workflow.Tasks.TryChangeStatusAsync(task,
                message.OperationFailed
                    ? OperationTaskStatus.Failed
                    : OperationTaskStatus.Completed
                , message.GetMessage(_workflow.WorkflowOptions.JsonSerializerOptions)))

                _log.LogDebug("Operation Workflow {operationId}, Task {taskId}: Status changed: {oldStatus} -> {newStatus}",
                    message.OperationId, message.TaskId, taskOldStatus, task.Status);



            if (message.TaskId == Data.PrimaryTaskId)
            {

                var newStatus = message.OperationFailed
                    ? OperationStatus.Failed
                    : OperationStatus.Completed;

                _log.LogDebug("Operation Workflow {operationId}: Primary task changed, updating operation status to {newStatus}",
                    message.OperationId, newStatus);


                if (await _workflow.Operations.TryChangeStatusAsync(op,
                        newStatus, message.GetMessage(_workflow.WorkflowOptions.JsonSerializerOptions), MessageContext.Current.Headers))
                {
                    await _workflow.Messaging.DispatchOperationStatusEventAsync(new OperationStatusEvent
                    {
                        OperationId = op.Id,
                        NewStatus = newStatus
                    });
                }

                _log.LogDebug("Operation Workflow {operationId}: Completing workflow",
                    message.OperationId);

                MarkAsComplete();
            }
        }

    }
}