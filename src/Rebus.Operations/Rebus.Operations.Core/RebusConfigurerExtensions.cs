using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Routing.TypeBased;

namespace Dbosoft.Rebus.Operations;

public static class RebusConfigurerExtensions
{
    public static TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder 
        AddOperations(this TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder typeBasedRouter, 
            string operationsOwner, string? eventsOwner = default)
    {
        eventsOwner ??= operationsOwner;

        return typeBasedRouter
            .Map<CreateOperationCommand>(operationsOwner)
            .Map<CreateNewOperationTaskCommand>(operationsOwner)
            .Map<OperationStatusEvent>(eventsOwner)
            .Map<OperationTaskProgressEvent>(eventsOwner)
            .Map<OperationTaskStatusEvent>(eventsOwner)
            .Map<OperationTaskAcceptedEvent>(eventsOwner)
            .Map<OperationTimeoutEvent>(eventsOwner);
    }
}