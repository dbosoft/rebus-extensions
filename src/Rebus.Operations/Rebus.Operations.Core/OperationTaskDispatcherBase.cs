

#nullable enable

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;
public abstract class OperationTaskDispatcherBase : IOperationTaskDispatcher
{
    private readonly IBus _bus;
    private readonly ILogger<OperationTaskDispatcherBase> _logger;

    protected OperationTaskDispatcherBase(IBus bus, ILogger<OperationTaskDispatcherBase> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, object command, object? additionalData = null)
    {
        return StartTask(operationId, initiatingTaskId, command, additionalData);
    }

    public ValueTask<IOperationTask?> StartNew<T>(Guid operationId, Guid initiatingTaskId, object? additionalData = default)
        where T : class, new()
    {
        return StartTask(operationId, initiatingTaskId, Activator.CreateInstance<T>(),
            additionalData);
    }

    public ValueTask<IOperationTask?> StartNew(Guid operationId, Guid initiatingTaskId, Type commandType, object? additionalData = default)
    {
        return StartTask(operationId, initiatingTaskId, commandType, additionalData);
    }

    protected abstract ValueTask<(IOperationTask, object)> CreateTask(Guid operationId, Guid initiatingTaskId, object command, object? additionalData);

    protected async ValueTask<IOperationTask?> StartTask(Guid operationId, Guid initiatingTaskId, object command, object? additionalData)
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

        await _bus.Send(taskMessage);

        _logger.LogDebug("Send new command of type {commandType}. Id: {operationId}, ParentTaskId: {parentTaskId}",
            taskCommand.GetType().Name, operationId, initiatingTaskId);

        return task;

    }


}