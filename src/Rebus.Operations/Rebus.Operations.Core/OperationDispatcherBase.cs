

#nullable enable

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations
{

    public abstract class OperationDispatcherBase : IOperationDispatcher
    {
        private readonly IBus _bus;
        private readonly ILogger<OperationDispatcherBase> _logger;

        protected OperationDispatcherBase(IBus bus, ILogger<OperationDispatcherBase> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public ValueTask<IOperation?> StartNew(object command)
        {
            return StartOperation(command, null);
        }

        public ValueTask<IOperation?> StartNew<T>(object? additionalData = default)
            where T : class, new()
        {
            return StartOperation( Activator.CreateInstance<T>(),additionalData);
        }


        public ValueTask<IOperation?> StartNew(Type commandType, object? additionalData = default)
        {
            return StartOperation(commandType,additionalData);
        }

        protected abstract ValueTask<(IOperation, object)> CreateOperation(object command, object? additionalData);

        protected async ValueTask<IOperation?> StartOperation(object command, object? additionalData)
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
            await _bus.Send(message);

            _logger.LogDebug("Send new command of type {commandType}. Id: {operationId}",
                taskCommand.GetType().Name, operation.Id);

            return operation;

        }

    }
}