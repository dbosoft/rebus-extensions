﻿
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;

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

    public ValueTask<IOperation?> StartNew(object command, object? additionalData = default, IDictionary<string,string>? additionalHeaders = null)
    {
        return StartOperation(command, additionalData, additionalHeaders);
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

    protected abstract ValueTask<(IOperation, object)> CreateOperation(object command, DateTimeOffset created, object? additionalData, IDictionary<string,string>? additionalHeaders);

    protected async ValueTask<IOperation?> StartOperation(object command, object? additionalData, IDictionary<string,string>? additionalHeaders = null)
    {
            
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var created = DateTimeOffset.Now;
        var (operation, taskCommand) =
            await CreateOperation(command, created, additionalData, additionalHeaders).ConfigureAwait(false);

        var commandJson = JsonSerializer.Serialize(taskCommand, _options.JsonSerializerOptions);

        var taskMessage = new CreateNewOperationTaskCommand(
            taskCommand.GetType().AssemblyQualifiedName 
            ?? throw new InvalidOperationException($"could not found qualified name of for type {taskCommand.GetType()}"),
            commandJson,
            operation.Id,
            operation.Id,
            Guid.NewGuid(),
            created);

        var message = new CreateOperationCommand { TaskMessage = taskMessage };
        await (string.IsNullOrWhiteSpace(_options.OperationsDestination)
                ? _bus.Send(message, additionalHeaders)
                : _bus.Advanced.Routing.Send(_options.OperationsDestination, message, additionalHeaders))
            .ConfigureAwait(false);

        _logger.LogDebug("Send new command of type {commandType}. Id: {operationId}",
            taskCommand.GetType().Name, operation.Id);

        return operation;
    }

}