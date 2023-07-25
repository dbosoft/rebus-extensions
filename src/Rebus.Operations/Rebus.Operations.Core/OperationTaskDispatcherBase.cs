

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;
public abstract class OperationTaskDispatcherBase : IOperationTaskDispatcher
{
    private readonly IBus _bus;
    private readonly WorkflowOptions _options;
    private readonly ILogger<OperationTaskDispatcherBase> _logger;

    protected OperationTaskDispatcherBase(IBus bus, WorkflowOptions options, ILogger<OperationTaskDispatcherBase> logger)
    {
        _bus = bus;
        _options = options;
        _logger = logger;
    }

    public ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, object command, 
        object? additionalData = null, IDictionary<string,string>? additionalHeaders = null)
    {
        return StartTask(operationId, initiatingTaskId, command, additionalData, additionalHeaders);
    }

    public ValueTask<IOperationTask?> StartNew<T>(Guid operationId, Guid initiatingTaskId, object? additionalData = default, IDictionary<string,string>? additionalHeaders = null)
        where T : class, new()
    {
        return StartTask(operationId, initiatingTaskId, Activator.CreateInstance<T>(),
            additionalData, additionalHeaders);
    }

    public ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, Type commandType, object? additionalData = default, IDictionary<string,string>? additionalHeaders = null)
    {
        return StartTask(operationId, initiatingTaskId, commandType, additionalData, additionalHeaders);
    }

    protected abstract ValueTask<(IOperationTask, object)> CreateTask(Guid operationId, Guid initiatingTaskId, object command, object? additionalData);

    protected async ValueTask<IOperationTask?> StartTask(Guid operationId, Guid initiatingTaskId, 
        object command, object? additionalData, IDictionary<string,string>? additionalHeaders = null)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var (task, taskCommand) = await CreateTask(operationId, initiatingTaskId, command, additionalData);
        var commandJson = JsonSerializer.Serialize(taskCommand);

        var taskMessage = new CreateNewOperationTaskCommand(
            taskCommand.GetType().AssemblyQualifiedName,
            commandJson,
            operationId,
            initiatingTaskId,
            task.Id);

        await (string.IsNullOrWhiteSpace(_options.OperationsDestination) 
            ? _bus.Send(taskMessage, additionalHeaders)
            : _bus.Advanced.Routing.Send(_options.OperationsDestination, taskMessage, additionalHeaders)) ;

        _logger.LogDebug("Send new command of type {commandType}. Id: {operationId}, ParentTaskId: {parentTaskId}",
            taskCommand.GetType().Name, operationId, initiatingTaskId);

        return task;

    }


}