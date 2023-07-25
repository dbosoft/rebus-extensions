using System;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Routing.TypeBased;
using SimpleInjector;

namespace Dbosoft.Rebus.Operations;

public static class SimpleInjectorExtensions
{
    
    public static Container AddRebusOperationsHandlers<TOpManager, TTaskManager>(this Container container)
        where TOpManager : IOperationManager
        where TTaskManager: IOperationTaskManager
    {
        return container.AddRebusOperationsHandlers(typeof(TOpManager), typeof(TTaskManager));
    }

    public static Container AddRebusOperationsHandlers(this Container container, 
        Type operationManagerType, 
        Type taskManagerType )
    {
        //workflow engine types
        container.Register(typeof(IOperationManager), operationManagerType, Lifestyle.Scoped);
        container.Register(typeof(IOperationTaskManager), taskManagerType, Lifestyle.Scoped);
        container.RegisterConditional<IOperationTaskDispatcher, DefaultOperationTaskDispatcher>(Lifestyle.Scoped,c=> !c.Handled);
        container.RegisterConditional<IOperationDispatcher, DefaultOperationDispatcher>(Lifestyle.Scoped,c=> !c.Handled);
        container.RegisterConditional<IWorkflow, DefaultWorkflow>(Lifestyle.Scoped,c=> !c.Handled);

        container.RegisterConditional<IOperationMessaging, RebusOperationMessaging>(Lifestyle.Scoped, c=> !c.Handled);
        container.RegisterConditional<IMessageEnricher, DefaultMessageEnricher>(Lifestyle.Scoped, c=> !c.Handled);

        container.Collection.Append(typeof(IHandleMessages<>), typeof(ProcessOperationSaga), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(OperationTaskProgressEventHandler), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(FailedOperationHandler<>), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(IncomingTaskMessageHandler<>), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(EmptyOperationStatusEventHandler), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(EmptyOperationTaskStatusEventHandler<>), Lifestyle.Scoped);

        return container;
    }
}