

#nullable enable

using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Retry.Simple;

namespace Dbosoft.Rebus.Operations.Workflow
{
    public class FailedOperationHandler<T> : IHandleMessages<IFailed<T>> where T: IOperationTaskMessage
    {
        private readonly IOperationMessaging _operationMessaging;

        private readonly WorkflowOptions _workflowOptions;
        private readonly ILogger<FailedOperationHandler<T>> _logger;

        public FailedOperationHandler(
            WorkflowOptions workflowOptions,
            ILogger<FailedOperationHandler<T>> logger, IOperationMessaging operationMessaging)
        {
            _workflowOptions = workflowOptions;
            _logger = logger;
            _operationMessaging = operationMessaging;
        }

        public async Task Handle(IFailed<T> failedMessage)
        {

            _logger.LogError("Task {taskId} failed with message: {failedMessage}",
                failedMessage.Message.TaskId, failedMessage.ErrorDescription
                );

            var failedTaskId = failedMessage.Message.TaskId;

            // assign errors on status events to initiating task
            if(failedMessage.Message is IOperationTaskStatusEvent statusEvent)
                failedTaskId = statusEvent.InitiatingTaskId;

            await _operationMessaging.DispatchTaskStatusEventAsync(
                OperationTaskStatusEvent.Failed(
                    failedMessage.Message.OperationId, failedMessage.Message.InitiatingTaskId,
                    failedTaskId, new ErrorData() { ErrorMessage = failedMessage.ErrorDescription },
                    _workflowOptions.JsonSerializerOptions));


        }
    }
}