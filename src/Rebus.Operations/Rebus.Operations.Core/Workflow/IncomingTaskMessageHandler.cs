using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Extensions;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;


namespace Dbosoft.Rebus.Operations.Workflow
{
    public class IncomingTaskMessageHandler<T> : IHandleMessages<OperationTaskSystemMessage<T>> where T: class, new()
    {
        private readonly IBus _bus;
        private readonly ILogger<IncomingTaskMessageHandler<T>> _logger;
        private readonly IMessageEnricher _messageEnricher;
        public IncomingTaskMessageHandler(IBus bus, ILogger<IncomingTaskMessageHandler<T>> logger, IMessageEnricher messageEnricher)
        {
            _bus = bus;
            _logger = logger;
            _messageEnricher = messageEnricher;
        }


        public async Task Handle(OperationTaskSystemMessage<T> taskMessage)
        {
            if(taskMessage.Message==null)
                throw new InvalidOperationException($"Operation Workflow {taskMessage.OperationId}/{taskMessage.TaskId}: missing command message");

            var headers = _messageEnricher.EnrichHeadersFromIncomingSystemMessage(taskMessage, MessageContext.Current.Headers);
            var reply = new OperationTaskAcceptedEvent
            {
                OperationId = taskMessage.OperationId,
                InitiatingTaskId = taskMessage.InitiatingTaskId,
                TaskId = taskMessage.TaskId,
                AdditionalData = _messageEnricher.EnrichTaskAcceptedReply(taskMessage),
                Created = DateTimeOffset.UtcNow
            };
            var replyAddress = MessageContext.Current.Headers.GetValueOrNull(Headers.ReturnAddress);

            if (replyAddress == null)
            {
                _logger.LogWarning($"Operation Workflow {taskMessage.OperationId}/{taskMessage.TaskId}: missing return address");
            }
            else
            {
                using var replyScope = new RebusTransactionScope();
                await _bus.Advanced.Routing.Send(replyAddress, reply).ConfigureAwait(false);
                _logger.LogTrace($"Accepted incoming operation message. Operation id: '{taskMessage.OperationId}'");
                await replyScope.CompleteAsync().ConfigureAwait(false);
            }

            await _bus.SendLocal(new OperationTask<T>(taskMessage.Message,
                    taskMessage.OperationId, taskMessage.InitiatingTaskId,
                    taskMessage.TaskId, taskMessage.Created)
                , headers
            ).ConfigureAwait(false);
        }
    }
}