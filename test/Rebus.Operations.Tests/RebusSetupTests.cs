using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class RebusSetupTests
{
    private readonly ITestOutputHelper _output;

    public RebusSetupTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public  TestRebusSetup SetupRebus(Action<BuiltinHandlerActivator, IWorkflow, IBus> configureActivator)
    {
        var rebusNetwork = new InMemNetwork();

        var opManager = new TestOperationManager();
        var taskManager = new TestTaskManager();
        
        var activator = new BuiltinHandlerActivator();
        var busStarter =
            Configure.With(activator)
                .Transport(cfg => cfg.UseInMemoryTransport(rebusNetwork, "main"))
                .Routing(r => r.TypeBased()
                    .Map<CreateOperationCommand>("main")
                    .Map<CreateNewOperationTaskCommand>("main")
                    .Map<OperationStatusEvent>("main")
                    .Map<OperationTaskProgressEvent>("main")
                    .Map<OperationTaskStatusEvent>("main")
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
        busStarter.Bus.Subscribe<OperationTaskStatusEvent>();
        //busStarter.Bus.Subscribe<OperationStatusEvent>();
        
        return new TestRebusSetup(bus, opDispatcher, opManager, taskManager);
        
    }

    [Fact]
    public async Task Command_is_processed_as_Operation()
    {
        TestCommandHandler? taskHandler = null;

        using var setup = SetupRebus(configureActivator: (activator, _, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, new DefaultMessageEnricher()));
            taskHandler = new TestCommandHandler(bus);
            activator.Register(() => taskHandler);
        });
        
        await setup.OperationDispatcher.StartNew<TestCommand>();
        await Task.Delay(1000);
        Assert.True(taskHandler!.Called);
        Assert.Single(setup.OperationManager.Operations);
    }

    [Fact]
    public async Task Command_is_processed_as_MultiStep_Workflow()
    {
        StepOneCommandHandler? stepOneHandler = null;
        StepTwoCommandHandler? stepTwoHandler = null;
        using var setup = SetupRebus(configureActivator: (activator, wf, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<MultiStepCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<MultiStepCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepOneCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepOneCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepTwoCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepTwoCommand>>.Instance, new DefaultMessageEnricher()));
            
            activator.Register(() => new MultiStepSaga(wf));

            stepOneHandler = new StepOneCommandHandler(bus);
            stepTwoHandler = new StepTwoCommandHandler(bus);
            activator.Register(() => stepOneHandler);
            activator.Register(() => stepTwoHandler);
        });
        
        await setup.OperationDispatcher.StartNew<MultiStepCommand>();
        await Task.Delay(1000);
        Assert.True(stepOneHandler!.Called);
        Assert.True(stepTwoHandler!.Called);
        Assert.Single(setup.OperationManager.Operations);
        Assert.Equal(3, setup.TaskManager.Tasks.Count);
        Assert.Equal(OperationStatus.Completed ,setup.OperationManager.Operations.First().Value.Status);

        foreach (var taskModel in setup.TaskManager.Tasks)
        {
            Assert.Equal(OperationTaskStatus.Completed, taskModel.Value.Status);
        }
    }
}