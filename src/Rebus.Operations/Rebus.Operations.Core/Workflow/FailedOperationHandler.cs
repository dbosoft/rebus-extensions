

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

        private readonly ILogger<FailedOperationHandler<T>> _logger;

        public FailedOperationHandler(ILogger<FailedOperationHandler<T>> logger, IOperationMessaging operationMessaging)
        {
            _logger = logger;
            _operationMessaging = operationMessaging;
        }

        public async Task Handle(IFailed<T> failedMessage)
        {

            _logger.LogError("Task {taskId} failed with message: {failedMessage}",
                failedMessage.Message.TaskId, failedMessage.ErrorDescription
                );

            await _operationMessaging.DispatchTaskStatusEventAsync(
                OperationTaskStatusEvent.Failed(
                    failedMessage.Message.OperationId, failedMessage.Message.InitiatingTaskId,
                    failedMessage.Message.TaskId, new ErrorData() { ErrorMessage = failedMessage.ErrorDescription }));


        }
    }
}