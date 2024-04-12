﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
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

        private static async void Resubmit(
            IBus bus,
            OperationTaskSystemMessage<T> taskMessage, IDictionary<string, string>? headers)
        {
            using var scope = new RebusTransactionScope();
            await bus.SendLocal(new OperationTask<T>(taskMessage.Message, 
                    taskMessage.OperationId, taskMessage.InitiatingTaskId,
                    taskMessage.TaskId, taskMessage.Created)
                , headers
            ).ConfigureAwait(false);

            await scope.CompleteAsync().ConfigureAwait(false);
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
                Created = taskMessage.Created
            };

            await _bus.Reply(reply).ConfigureAwait(false);
            _logger.LogTrace($"Accepted incoming operation message. Operation id: '{taskMessage.OperationId}'");

            Resubmit(_bus, taskMessage, headers);
        }
    }
}