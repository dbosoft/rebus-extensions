using System;
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
        ILogger<DefaultOperationDispatcher> logger,
        IOperationManager operationManager) : base(bus, logger)
    {
        _operationManager = operationManager;
    }

    protected override async ValueTask<(IOperation, object)> CreateOperation(object command, object? additionalData)
    {
        return (await _operationManager.GetOrCreateAsync(Guid.NewGuid(), command), command);
    }
}