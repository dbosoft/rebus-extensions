using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Dbosoft.Rebus.OperationsDB.Tests.Models;
using JetBrains.Annotations;
using MartinCostello.Logging.XUnit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Sagas;
using Rebus.Sagas.Exclusive;
using Rebus.Transport.InMem;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class DatabaseTests : IClassFixture<DatabaseTests.DeleteDb>
{
    private readonly ITestOutputHelper _outputHelper;
 
    public DatabaseTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;

    }

    private async Task SetupAndRunWorkflow(
        int workers, 
        int timeout,
        Func<IOperationDispatcher, Task<IEnumerable<(IOperation?, OperationStatus)>>> starter,
        Func<IServiceProvider,IOperation?, Task>? validator = null)
    
    {
        var inMemNetwork = new InMemNetwork();
        var workflowOptions = new WorkflowOptions
        {
            DispatchMode = WorkflowEventDispatchMode.Send,
            OperationsDestination = "workflow"
        };
        
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
 
       container.Collection.Register(typeof(IHandleMessages<>), typeof(DatabaseTests).Assembly);
        
        container.RegisterInstance(workflowOptions);
        container.AddRebusOperationsHandlers<MyOperationManager, MyOperationTaskManager>();

        container.Register(typeof(IStateStoreRepository<>), typeof(StateStoreRepository<>), Lifestyle.Scoped);
        
        var contextOptions = new DbContextOptionsBuilder<StateStoreContext>()
            .UseSqlite("Data Source=state.db")
            .Options;

        await using (var setupContext = new StateStoreContext(contextOptions))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }
        
        container.Register(() => new StateStoreContext(contextOptions), Lifestyle.Scoped);
        container.Register<IRebusUnitOfWork, StateStoreDbUnitOfWork>(Lifestyle.Scoped);
        
        container.ConfigureRebus(configurer => configurer
            .Transport(t => t.UseInMemoryTransport(inMemNetwork, "workflow"))
            .Routing(r => r.TypeBased().AddOperations("workflow"))
            .Options(x =>
            {
                x.SimpleRetryStrategy(secondLevelRetriesEnabled: true, errorDetailsHeaderMaxLength: 5);
                x.SetNumberOfWorkers(workers);
                x.EnableSimpleInjectorUnitOfWork();
            })
            .Logging(x=>x.MicrosoftExtensionsLogging(new XUnitLogger("rebus", _outputHelper, 
                new XUnitLoggerOptions())))
            .Sagas(s =>
            {
                s.StoreInMemory();
                s.EnforceExclusiveAccess();
            })
             .Start());

        _ = new HostBuilder()
            .ConfigureLogging(l => l.AddXUnit(_outputHelper))
            .ConfigureServices(s=>s.AddSimpleInjector(container,
                cfg => cfg.AddLogging()))
            .Build()
            .UseSimpleInjector(container);

        (IOperation?, OperationStatus)[] operations;
        await using (var startScope = AsyncScopedLifestyle.BeginScope(container))
        {
            // starts the bus
            _ = startScope.GetInstance<IBus>();
            
            await using var startContext = startScope.GetRequiredService<StateStoreContext>();
            var dispatcher = startScope.GetRequiredService<IOperationDispatcher>();

            operations = (await starter(dispatcher)).ToArray();
            await startContext.SaveChangesAsync();

        }
        
        var cancelTokenSource = new CancellationTokenSource(timeout);
        var pendingOperations = operations.Select(x=>x.Item1?.Id).ToList();
        
        while (!cancelTokenSource.IsCancellationRequested)
        {
            await using var scope = AsyncScopedLifestyle.BeginScope(container);
            var repository = scope.GetInstance<IStateStoreRepository<OperationModel>>();

            foreach (var id in pendingOperations.ToArray())
            {
                if(id == null)
                    pendingOperations.Remove(id);
                else
                {
                    await Task.Delay(500, CancellationToken.None);
                    var currentOperation = await repository.GetByIdAsync(id.GetValueOrDefault(), CancellationToken.None);
                    if (currentOperation == null)
                        throw new NullReferenceException($"Operation {id} is null");

                    if (currentOperation.Status is OperationStatus.Completed or OperationStatus.Failed )
                    {
                        pendingOperations.Remove(id);
                    }
                }
            }

            if (pendingOperations.Count == 0)
                break;
        }
        await using var validatorScope = AsyncScopedLifestyle.BeginScope(container);
        
        foreach (var (operation, expectedStatus) in operations)
        {
            var repository = validatorScope.GetInstance<IStateStoreRepository<OperationModel>>();
            if (operation == null)
                throw new NullReferenceException($"Operation is null is null");

            var currentOperation = await repository.GetByIdAsync(operation.Id, CancellationToken.None);
            if (currentOperation == null)
                throw new NullReferenceException($"Operation {operation.Id} is null");

            Assert.Equal(expectedStatus, currentOperation.Status);
            validator?.Invoke(validatorScope, currentOperation);

        }
    }
    
    [Theory]
    [InlineData(1, 1, 5000)]
    [InlineData(1, 5, 5000)]
    [InlineData(3, 5, 10000)]
    [InlineData(2, 10, 10000)]
    public async Task Runs_and_reports_a_simple_Workflow(int workers, int commands, int timeout)
    {
        await SetupAndRunWorkflow(workers,timeout, async d =>
        {
            var result = new List<(IOperation?, OperationStatus)>();
            foreach (var _ in Enumerable.Range(0, commands))
            {
                result.Add((await d.StartNew<SimpleCommand>(), OperationStatus.Completed));
            }

            return result;
        });
        
        
    }
    
    [Theory]
    [InlineData(1, 1, 5000)]
    [InlineData(3, 5, 8000)]
    [InlineData(5, 13, 15000)]
    [InlineData(5, 30, 40000)]
    public async Task Runs_and_reports_a_complex_Workflow(int workers, int commands, int timeout)
    {
        await SetupAndRunWorkflow(workers,timeout, async d =>
        {
            var result = new List<(IOperation?, OperationStatus)>();
            foreach (var _ in Enumerable.Range(0, commands))
            {
                result.Add((await d.StartNew<InitialSagaCommand>(), OperationStatus.Completed));
            }

            return result;
        });
        
        
    }
    

    [UsedImplicitly]
    private class DeleteDb
    {
        public DeleteDb()
        {
            if(File.Exists("state.db"))
                File.Delete("state.db");
        }
    }
    
}

public class InitialSagaCommand
{
        
}
    
public class NestedSagaCommand
{
        
}

public class SubCommand1
{
        
}

public class SubCommand2
{
}

public class SubCommand3
{
        
}

public class InitialSagaData : TaskWorkflowSagaData
{
    public bool SubCommand1Completed { get; set; }
    public bool SagaCompleted { get; set; }
}

public class InitialSaga : OperationTaskWorkflowSaga<InitialSagaCommand, InitialSagaData>, 
    IHandleMessages<OperationTaskStatusEvent<NestedSagaCommand>>,
    IHandleMessages<OperationTaskStatusEvent<SubCommand1>>
    
{
    public InitialSaga(IWorkflow workflowEngine) : base(workflowEngine)
    {
    }
    
    protected override void CorrelateMessages(ICorrelationConfig<InitialSagaData> config)
    {
        base.CorrelateMessages(config);

        config.Correlate<OperationTaskStatusEvent<NestedSagaCommand>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<SubCommand1>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
    }

    protected override async Task Initiated(InitialSagaCommand message)
    {
        await StartNewTask<NestedSagaCommand>();
        await StartNewTask<SubCommand1>();
    }
    

    public async Task Handle(OperationTaskStatusEvent<SubCommand1> message)
    {
        await FailOrRun<SubCommand1>(message, async () =>
        {
            Data.SubCommand1Completed = true;

            if (Data.SubCommand1Completed && Data.SagaCompleted)
                await Complete();
        });
    }

    public async Task Handle(OperationTaskStatusEvent<NestedSagaCommand> message)
    {
        await FailOrRun<NestedSagaCommand>(message, async () =>
        {
            Data.SagaCompleted = true;

            if (Data.SubCommand1Completed && Data.SagaCompleted)
                await Complete();
        });
    }


}

public class NestedSagaData : TaskWorkflowSagaData
{
    public bool SubCommand2Completed { get; set; }
    public bool SubCommand3Completed { get; set; }
}

public class NestedSaga : OperationTaskWorkflowSaga<NestedSagaCommand, NestedSagaData>, 
    IHandleMessages<OperationTaskStatusEvent<SubCommand2>>,
    IHandleMessages<OperationTaskStatusEvent<SubCommand3>>
    
{
    protected override void CorrelateMessages(ICorrelationConfig<NestedSagaData> config)
    {
        base.CorrelateMessages(config);

        config.Correlate<OperationTaskStatusEvent<SubCommand2>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
        config.Correlate<OperationTaskStatusEvent<SubCommand3>>(m => m.InitiatingTaskId,
            d => d.SagaTaskId);
    }

    protected override async Task Initiated(NestedSagaCommand message)
    {
        await StartNewTask<SubCommand2>();
        await StartNewTask<SubCommand3>();
    }
    

    public async Task Handle(OperationTaskStatusEvent<SubCommand2> message)
    {
        await FailOrRun<SubCommand2>(message, async () =>
        {
            Data.SubCommand2Completed = true;

            if (Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete();
        });
    }

    public async Task Handle(OperationTaskStatusEvent<SubCommand3> message)
    {
        await FailOrRun<SubCommand3>(message, async () =>
        {
            Data.SubCommand3Completed = true;

            if (Data.SubCommand2Completed && Data.SubCommand3Completed)
                await Complete();
        });
    }

    public NestedSaga(IWorkflow workflowEngine) : base(workflowEngine)
    {
    }
}

public class Subcommand1Handler : IHandleMessages<OperationTask<SubCommand1>>
{
    private readonly ITaskMessaging _taskMessaging;

    public Subcommand1Handler(ITaskMessaging taskMessaging)
    {
        _taskMessaging = taskMessaging;
    }
    
    public async Task Handle(OperationTask<SubCommand1> message)
    {
        await _taskMessaging.ProgressMessage(message, "started task Subcommand1");
        await Task.Delay(1000);
        await _taskMessaging.CompleteTask(message);
    }
}

public class Subcommand2Handler : IHandleMessages<OperationTask<SubCommand2>>
{
    private readonly ITaskMessaging _taskMessaging;

    public Subcommand2Handler(ITaskMessaging taskMessaging)
    {
        _taskMessaging = taskMessaging;
    }
    
    public async Task Handle(OperationTask<SubCommand2> message)
    {
        await _taskMessaging.ProgressMessage(message, "started task Subcommand2");
        await Task.Delay(3000);
        await _taskMessaging.CompleteTask(message);
    }
}

public class Subcommand3Handler : IHandleMessages<OperationTask<SubCommand3>>
{
    private readonly ITaskMessaging _taskMessaging;

    public Subcommand3Handler(ITaskMessaging taskMessaging)
    {
        _taskMessaging = taskMessaging;
    }
    
    public async Task Handle(OperationTask<SubCommand3> message)
    {
        await _taskMessaging.ProgressMessage(message, "started task Subcommand3");
        await Task.Delay(500);
        await _taskMessaging.ProgressMessage(message, "progress Subcommand3");
        await Task.Delay(500);
        await _taskMessaging.ProgressMessage(message, "started task Subcommand3");
        await Task.Delay(500);
        await _taskMessaging.CompleteTask(message);
    }
}

