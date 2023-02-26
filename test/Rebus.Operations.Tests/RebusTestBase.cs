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
        
        Action<BuiltinHandlerActivator, IWorkflow, IBus> configureActivator)
    {
        var rebusNetwork = new InMemNetwork();

        var opManager = new TestOperationManager();
        var taskManager = new TestTaskManager();
        
        var activator = new BuiltinHandlerActivator();
        var busStarter =
            Configure.With(activator)
                .Options(o => 
                    o.SimpleRetryStrategy(maxDeliveryAttempts:1, secondLevelRetriesEnabled:true))
                .Transport(cfg => cfg.UseInMemoryTransport(rebusNetwork, "main"))
                .Routing(r => r.TypeBased()
                    .Map<CreateOperationCommand>("main")
                    .Map<CreateNewOperationTaskCommand>("main")
                    .Map<OperationStatusEvent>("main")
                    .Map<OperationTaskProgressEvent>("main")
                    .Map<OperationTaskStatusEvent>("main")
                    .Map<OperationTaskAcceptedEvent>("main")
                    .Map<OperationTimeoutEvent>("main")
                )
                .Sagas(x => x.StoreInMemory())
                .Subscriptions(x=>x.StoreInMemory())
                .Logging(x=>x.Use(new RebusTestLogging(_output)))
                .Create();

        
        var opDispatcher = new DefaultOperationDispatcher(busStarter.Bus,
            NullLogger<DefaultOperationDispatcher>.Instance, opManager);

        var taskDispatcher = new DefaultOperationTaskDispatcher(busStarter.Bus,
            NullLogger<DefaultOperationTaskDispatcher>.Instance,
            opManager, taskManager);
        
        var workflow = new DefaultWorkflow(
            opManager, taskManager, new RebusOperationMessaging(busStarter.Bus,
                opDispatcher, taskDispatcher));

        activator.Register(() => new ProcessOperationSaga(workflow, NullLogger.Instance));
        activator.Register(() =>
            new OperationTaskProgressEventHandler(workflow, NullLogger<OperationTaskProgressEventHandler>.Instance));
        
        configureActivator(activator,workflow, busStarter.Bus);

        var bus = busStarter.Start();
        await OperationsSetup.SubscribeEvents(busStarter.Bus);
        
        return new TestRebusSetup(bus, opDispatcher, opManager, taskManager);
        
    }

}