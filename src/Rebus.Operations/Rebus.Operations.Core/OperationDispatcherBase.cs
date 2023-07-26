

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations
{

    public abstract class OperationDispatcherBase : IOperationDispatcher
    {
        private readonly IBus _bus;
        private readonly WorkflowOptions _options;
        private readonly ILogger<OperationDispatcherBase> _logger;

        protected OperationDispatcherBase(IBus bus, WorkflowOptions options, ILogger<OperationDispatcherBase> logger)
        {
            _bus = bus;
            _options = options;
            _logger = logger;
        }

        public ValueTask<IOperation?> StartNew(object command, IDictionary<string,string>? additionalHeaders = null)
        {
            return StartOperation(command, null, additionalHeaders);
        }

        public ValueTask<IOperation?> StartNew<T>(object? additionalData = default, IDictionary<string,string>? additionalHeaders = null)
            where T : class, new()
        {
            return StartOperation( Activator.CreateInstance<T>(),additionalData, additionalHeaders);
        }


        public ValueTask<IOperation?> StartNew(Type commandType, object? additionalData = default, IDictionary<string,string>? additionalHeaders = null)
        {
            return StartOperation(commandType,additionalData, additionalHeaders);
        }

        protected abstract ValueTask<(IOperation, object)> CreateOperation(object command, object? additionalData);

        protected async ValueTask<IOperation?> StartOperation(object command, object? additionalData, IDictionary<string,string>? additionalHeaders = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var(operation, taskCommand) = await CreateOperation(command, additionalData);
            
            var commandJson = JsonSerializer.Serialize(taskCommand);

            var taskMessage = new CreateNewOperationTaskCommand(
                taskCommand.GetType().AssemblyQualifiedName,
                commandJson,
                operation.Id,
                operation.Id,
                Guid.NewGuid());

            var message = new CreateOperationCommand { TaskMessage = taskMessage };
            await (string.IsNullOrWhiteSpace(_options.OperationsDestination)
                ? _bus.Send(message, additionalHeaders)
                : _bus.Advanced.Routing.Send(_options.OperationsDestination, message, additionalHeaders));

            _logger.LogDebug("Send new command of type {commandType}. Id: {operationId}",
                taskCommand.GetType().Name, operation.Id);

            return operation;

        }

    }
}