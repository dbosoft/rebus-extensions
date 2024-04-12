using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Pipeline;

namespace Dbosoft.Rebus.Operations.Workflow
{
    [UsedImplicitly]
    public class OperationTaskProgressEventHandler : IHandleMessages<OperationTaskProgressEvent>
    {
        private readonly IWorkflow _workflow;
        private readonly ILogger<OperationTaskProgressEventHandler> _logger;

        public OperationTaskProgressEventHandler(IWorkflow workflow, ILogger<OperationTaskProgressEventHandler> logger)
        {
            _workflow = workflow;
            _logger = logger;
        }


        public async Task Handle(OperationTaskProgressEvent message)
        {
            _logger.LogDebug($"Received operation task progress event. Id : '{message.OperationId}/{message.TaskId}'");

            var operation = await _workflow.Operations
                .GetByIdAsync(message.OperationId)
                .ConfigureAwait(false);

            var task = await _workflow.Tasks
                .GetByIdAsync(message.TaskId)
                .ConfigureAwait(false);


            if (operation != null && task!=null)
            {
                await _workflow.Operations.AddProgressAsync(
                    message.Id,
                    message.Timestamp,
                    operation,
                    task,
                    message.Data, MessageContext.Current.Headers).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning($"Received operation task progress event for a unknown operation task. Id : '{message.OperationId}/{message.TaskId}'", new
                {
                    message.OperationId,
                    message.TaskId,
                    message.Data,
                    message.Timestamp,
                });

            }
            
        }
    }
}