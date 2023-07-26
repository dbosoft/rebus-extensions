using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;

public class DefaultOperationTaskDispatcher : OperationTaskDispatcherBase
{
    private readonly IOperationManager _operationManager;
    private readonly IOperationTaskManager _operationTaskManager;


    public DefaultOperationTaskDispatcher(
        IBus bus, 
        WorkflowOptions workflowOptions,
        ILogger<DefaultOperationTaskDispatcher> logger,
        IOperationManager operationManager, IOperationTaskManager operationTaskManager) : base(bus, workflowOptions, logger)
    {
        _operationManager = operationManager;
        _operationTaskManager = operationTaskManager;
    }

    protected override async ValueTask<(IOperationTask, object)> CreateTask(Guid operationId, Guid initiatingTaskId, 
        object command, object? additionalData, IDictionary<string,string>? additionalHeaders)
    {
        var op = await _operationManager.GetByIdAsync(operationId);
        if (op == null)
        {
            throw new ArgumentException($"Operation {operationId} not found", nameof(operationId));
        }

        return (await _operationTaskManager.GetOrCreateAsync(op, command, Guid.NewGuid(), initiatingTaskId), command);
    }
}