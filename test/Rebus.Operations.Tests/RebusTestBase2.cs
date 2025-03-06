using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Dbosoft.Rebus.Operations.Tests;

public abstract class RebusTestBase2 : IDisposable
{
    private readonly BuiltinHandlerActivator _activator = new();
    private readonly IMessageEnricher _messageEnricher;
    private readonly IBus _bus;
    private readonly IBusStarter _busStarter;
    private readonly WorkflowEventDispatchMode _dispatchMode;
    private readonly WorkflowOptions _workflowOptions;
    private readonly IWorkflow _workflow;
    private readonly ITaskMessaging _taskMessaging;
    private readonly ITestOutputHelper _output;
    protected internal readonly TestOperationStore Store = new();
    protected internal readonly TestTracer Tracer = new();
    protected internal readonly TestOperationManager _operationManager;
    protected internal readonly TestTaskManager _taskManager;
    private readonly IOperationDispatcher _operationDispatcher;


    protected RebusTestBase2(
        ITestOutputHelper output,
        WorkflowEventDispatchMode dispatchMode,
        string eventDestination)
        : this(output, dispatchMode, eventDestination, new DefaultMessageEnricher())
    {
    }

    protected RebusTestBase2(
        ITestOutputHelper output,
        WorkflowEventDispatchMode dispatchMode,
        string eventDestination,
        IMessageEnricher messageEnricher)
    {
        _operationManager = new TestOperationManager(Store);
        _taskManager = new TestTaskManager(Store);
        _messageEnricher = messageEnricher;
        _output = output;
        _workflowOptions = new WorkflowOptions
        {
            DispatchMode = dispatchMode,
            EventDestination = eventDestination,
            OperationsDestination = eventDestination
        };

        var rebusNetwork = new InMemNetwork();

        _busStarter = Configure.With(_activator)
            .Options(o =>
            {
                o.RetryStrategy(maxDeliveryAttempts: 1, secondLevelRetriesEnabled: true);
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
            .Logging(x => x.Use(new RebusTestLogging(_output)))
            .Create();
        _bus = _busStarter.Bus;

        _operationDispatcher = new DefaultOperationDispatcher(
            _bus, _workflowOptions,
            NullLogger<DefaultOperationDispatcher>.Instance,
            _operationManager);
        var taskDispatcher = new DefaultOperationTaskDispatcher(
            _bus, _workflowOptions,
            NullLogger<DefaultOperationTaskDispatcher>.Instance,
            _operationManager, _taskManager);
        
        _workflow = new DefaultWorkflow(
            _workflowOptions, _operationManager, _taskManager,
            new RebusOperationMessaging(_bus, _operationDispatcher, taskDispatcher, messageEnricher, _workflowOptions));
        _taskMessaging = new RebusTaskMessaging(_bus, _workflowOptions);

        _activator.Register(() => new ProcessOperationSaga(_workflow, NullLogger.Instance));
        _activator.Register(() => new OperationTaskProgressEventHandler(
            _workflow, NullLogger<OperationTaskProgressEventHandler>.Instance));
        _activator.Register(() => new EmptyOperationStatusEventHandler());
    }





    protected void AddTaskHandler<TCommand, THandler>()
        where TCommand : class, new()
        where THandler : class, IHandleMessages<OperationTask<TCommand>>
    {
        _activator.Register(() => new IncomingTaskMessageHandler<TCommand>(
            _bus, NullLogger<IncomingTaskMessageHandler<TCommand>>.Instance, _messageEnricher));
        _activator.Register(() => new FailedOperationHandler<OperationTask<TCommand>>(
            _workflow.WorkflowOptions,
            NullLogger<FailedOperationHandler<OperationTask<TCommand>>>.Instance,
            _workflow.Messaging));
        _activator.Register(() => (THandler)Activator.CreateInstance(typeof(THandler), _taskMessaging, Tracer)!);
    }

    protected void AddSaga<TCommand, TSaga, TSagaData>()
        where TSaga : OperationTaskWorkflowSaga<TCommand, TSagaData>
        where TCommand : class, new()
        where TSagaData : TaskWorkflowSagaData, new()
    {
        _activator.Register(() => new IncomingTaskMessageHandler<TCommand>(
            _bus, NullLogger<IncomingTaskMessageHandler<TCommand>>.Instance, _messageEnricher));
        _activator.Register(() => new FailedOperationHandler<OperationTask<TCommand>>(
            _workflow.WorkflowOptions,
            NullLogger<FailedOperationHandler<OperationTask<TCommand>>>.Instance,
            _workflow.Messaging));
        _activator.Register(() => (TSaga)Activator.CreateInstance(typeof(TSaga), _workflow, Tracer)!);
    }

    protected async Task StartBus()
    {
        _busStarter.Start();
        if (_dispatchMode is not WorkflowEventDispatchMode.Publish)
            return;

        await OperationsSetup.SubscribeEvents(_bus, _workflowOptions);
    }

    protected ValueTask<IOperation?> StartOperation<TCommand>()
        where TCommand : class, new()
    {
        return _operationDispatcher.StartNew<TCommand>();
    }

    protected async Task WaitForOperation(
        Guid operationId,
        TimeSpan? timeout = null)
    {
        using var tokenSource = new CancellationTokenSource(
            timeout.GetValueOrDefault(TimeSpan.FromSeconds(1000)));

        try
        {
            IOperation? operation;
            do
            {
                await Task.Delay(100, tokenSource.Token);
                operation = await _operationManager.GetByIdAsync(operationId);
                Assert.NotNull(operation);
                if (operation is null)
                    throw new XunitException($"Operation {operationId} does not exists");
            } while (operation.Status is OperationStatus.Queued or OperationStatus.Running);

        }
        catch (TaskCanceledException)
        {
            // Nothing to do
        }
    }

    public void Dispose()
    {
        _bus?.Dispose();
        _activator.Dispose();
    }
}
