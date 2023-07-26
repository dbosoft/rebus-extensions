using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public abstract class RebusTestBase
{
    private readonly ITestOutputHelper _output;

    protected RebusTestBase(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task<TestRebusSetup> SetupRebus(
        bool sendMode, string eventDestination,
        Action<BuiltinHandlerActivator, IWorkflow, ITaskMessaging, IBus> configureActivator)
    {
        var rebusNetwork = new InMemNetwork();

        var opManager = new TestOperationManager();
        var taskManager = new TestTaskManager();
        
        var activator = new BuiltinHandlerActivator();
        var busStarter =
            Configure.With(activator)
                .Options(o =>
                {
                    o.SimpleRetryStrategy(maxDeliveryAttempts: 1, secondLevelRetriesEnabled: true);
                })
                .Transport(cfg => cfg.UseInMemoryTransport(rebusNetwork, "main"))
                .Routing(r =>
                {
                    if (string.IsNullOrWhiteSpace(eventDestination))
                    {
                        r.TypeBased().AddOperations("main");
                    }
                })
                .Sagas(x => x.StoreInMemory())
                .Subscriptions(x=>x.StoreInMemory(new InMemorySubscriberStore()))
                .Logging(x=>x.Use(new RebusTestLogging(_output)))
                .Create();

        var workflowOptions = new WorkflowOptions
        {
            DispatchMode = sendMode ? WorkflowEventDispatchMode.Send : WorkflowEventDispatchMode.Publish,
            EventDestination = eventDestination,
            OperationsDestination = eventDestination
        };
        
        var opDispatcher = new DefaultOperationDispatcher(busStarter.Bus,workflowOptions,
            NullLogger<DefaultOperationDispatcher>.Instance, opManager);

        var taskDispatcher = new DefaultOperationTaskDispatcher(busStarter.Bus,
            workflowOptions,
            NullLogger<DefaultOperationTaskDispatcher>.Instance,
            opManager, taskManager);
        
        var workflow = new DefaultWorkflow(
            opManager, taskManager, new RebusOperationMessaging(busStarter.Bus,
                opDispatcher, taskDispatcher,workflowOptions ));

        var taskMessaging = new RebusTaskMessaging(busStarter.Bus, workflowOptions);
        
        activator.Register(() => new ProcessOperationSaga(workflow, NullLogger.Instance));
        activator.Register(() =>
            new OperationTaskProgressEventHandler(workflow, NullLogger<OperationTaskProgressEventHandler>.Instance));
        
        configureActivator(activator,workflow, taskMessaging, busStarter.Bus);

        var bus = busStarter.Start();
        
        if(!sendMode)
            await OperationsSetup.SubscribeEvents(busStarter.Bus, workflowOptions);
        
        return new TestRebusSetup(bus, opDispatcher, opManager, taskManager);
        
    }

}