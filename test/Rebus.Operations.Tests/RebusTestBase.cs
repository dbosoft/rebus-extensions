using Dbosoft.Rebus.Operations.Tests.Data;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public abstract class RebusTestBase : IDisposable
{
    protected internal readonly TestOperationStore Store = new();
    protected internal readonly TestTrace Trace = new();

    private readonly BuiltinHandlerActivator _activator = new();
    private readonly IMessageEnricher _messageEnricher;
    private readonly IBus _bus;
    private readonly IBusStarter _busStarter;
    private readonly WorkflowEventDispatchMode _dispatchMode;
    private readonly IWorkflow _workflow;
    private readonly ITaskMessaging _taskMessaging;
    private readonly IOperationManager _operationManager;
    private readonly IOperationDispatcher _operationDispatcher;

    protected RebusTestBase(
        ITestOutputHelper output,
        WorkflowEventDispatchMode dispatchMode,
        bool useTypeBasedRouting)
        : this(output, dispatchMode, useTypeBasedRouting, new DefaultMessageEnricher())
    {
    }

    protected RebusTestBase(
        ITestOutputHelper output,
        WorkflowEventDispatchMode dispatchMode,
        bool useTypeBasedRouting,
        IMessageEnricher messageEnricher)
    {
        _dispatchMode = dispatchMode;
        _messageEnricher = messageEnricher;
        _operationManager = new TestOperationManager(Store);
        var taskManager = new TestOperationTaskManager(Store);
        var workflowOptions = new WorkflowOptions
        {
            DispatchMode = dispatchMode,
            EventDestination = useTypeBasedRouting ? null : "main",
            OperationsDestination = useTypeBasedRouting ? null : "main"
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
                if (useTypeBasedRouting)
                {
                    r.TypeBased().AddOperations("main");
                }
            })
            .Sagas(x => x.StoreInMemory())
            .Logging(x => x.Use(new RebusTestLogging(output)))
            .Create();
        _bus = _busStarter.Bus;

        _operationDispatcher = new DefaultOperationDispatcher(
            _bus, workflowOptions,
            NullLogger<DefaultOperationDispatcher>.Instance,
            _operationManager);
        var taskDispatcher = new DefaultOperationTaskDispatcher(
            _bus, workflowOptions,
            NullLogger<DefaultOperationTaskDispatcher>.Instance,
            _operationManager, taskManager);
        
        _workflow = new DefaultWorkflow(
            workflowOptions, _operationManager, taskManager,
            new RebusOperationMessaging(_bus, _operationDispatcher, taskDispatcher, messageEnricher, workflowOptions));
        _taskMessaging = new RebusTaskMessaging(_bus, workflowOptions);

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
        _activator.Register(() => (THandler)Activator.CreateInstance(typeof(THandler), _taskMessaging, Trace)!);
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
        _activator.Register(() => (TSaga)Activator.CreateInstance(typeof(TSaga), _workflow, Trace)!);
    }

    protected async Task StartBus()
    {
        _busStarter.Start();
        if (_dispatchMode is not WorkflowEventDispatchMode.Publish)
            return;

        await OperationsSetup.SubscribeEvents(_bus, _workflow.WorkflowOptions);
    }

    protected ValueTask<IOperation?> StartOperation<TCommand>(
        object? additionalData = null,
        IDictionary<string, string>? additionalHeaders = null)
        where TCommand : class, new()
    {
        return _operationDispatcher.StartNew<TCommand>(additionalData, additionalHeaders);
    }

    protected Task WaitForOperation(Guid operationId)
    {
        return _operationManager.WaitForOperation(operationId);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        
        _bus.Dispose();
        _activator.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
