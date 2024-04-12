using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Dbosoft.Rebus.Operations;

#nullable enable

public class DefaultOperationDispatcher : OperationDispatcherBase
{
    private readonly IOperationManager _operationManager;

    public DefaultOperationDispatcher(
        IBus bus,
        WorkflowOptions workflowOptions,
        ILogger<DefaultOperationDispatcher> logger,
        IOperationManager operationManager) : base(bus, workflowOptions, logger)
    {
        _operationManager = operationManager;
    }

    protected override async ValueTask<(IOperation, object)> CreateOperation(object command,
        DateTimeOffset created,
        object? additionalData,
        IDictionary<string,string>? additionalHeaders)
    {
        return (await _operationManager.GetOrCreateAsync(Guid.NewGuid(), command, 
            created,
            additionalData,additionalHeaders).ConfigureAwait(false), command);
    }
}