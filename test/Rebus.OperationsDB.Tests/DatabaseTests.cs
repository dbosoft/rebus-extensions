using Dbosoft.Rebus.Operations;
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
            DispatchMode = WorkflowEventDispatchMode.Publish,
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
                x.SimpleRetryStrategy(secondLevelRetriesEnabled: true, errorDetailsHeaderMaxLength: 5, maxDeliveryAttempts: 5);
                x.SetNumberOfWorkers(workers);
                x.SetMaxParallelism(workers);
                x.EnableSimpleInjectorUnitOfWork();
            })
            .Logging(x=>x.MicrosoftExtensionsLogging(new XUnitLogger("rebus", _outputHelper, 
                new XUnitLoggerOptions())))
            .Subscriptions(c => c.StoreInMemory())
            .Sagas(s =>
            {
                s.StoreInMemory();
                s.EnforceExclusiveAccess();
            })
             .Start());

        _ = new HostBuilder()
            .ConfigureLogging(l =>
            {
                l.AddXUnit(_outputHelper);
                l.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices(s=>s.AddSimpleInjector(container,
                cfg => cfg.AddLogging()))
            .Build()
            .UseSimpleInjector(container);

        (IOperation?, OperationStatus)[] operations;
        await using (var startScope = AsyncScopedLifestyle.BeginScope(container))
        {
            // starts the bus
            var bus = startScope.GetInstance<IBus>();
            await OperationsSetup.SubscribeEvents(bus, workflowOptions);
            
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
    [InlineData(5, 13, 20000)]
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