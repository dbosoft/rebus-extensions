using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Tests.Data;
using Dbosoft.Rebus.Operations.Tests.Handlers;
using Dbosoft.Rebus.Operations.Tests.Sagas;
using Dbosoft.Rebus.Operations.Workflow;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Handlers;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.SimpleInjector.Tests;

public class SimpleInjectorWorkflowTests : SimpleInjectorTestBase
{
    private readonly TestOperationStore _testStore = new();
    private readonly TestTrace _trace = new();

    public SimpleInjectorWorkflowTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Theory]
    [InlineData(WorkflowEventDispatchMode.Publish, false)]
    [InlineData(WorkflowEventDispatchMode.Publish, true)]
    [InlineData(WorkflowEventDispatchMode.Send, false)]
    [InlineData(WorkflowEventDispatchMode.Send, true)]
    public async Task MultiStep_Operation_is_processed(
        WorkflowEventDispatchMode dispatchMode,
        bool useTypeBasedRouting)
    {
        var sc = new ServiceCollection();
        sc.AddLogging();
        
        await using var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        sc.AddSimpleInjector(container, cfg => cfg.AddLogging());

        container.RegisterInstance(_testStore);
        container.RegisterInstance(_trace);

        SetupRebus(container, dispatchMode, useTypeBasedRouting);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(MultiStepSaga));
        container.Collection.Append(typeof(IHandleMessages<>), typeof(WithoutResponseCommandHandler), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(WithResponseCommandHandler), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(FinalStepCommandHandler), Lifestyle.Scoped);
        
        var sp = sc.BuildServiceProvider();
        sp.UseSimpleInjector(container);
        
        container.Verify();

        await using var scope = AsyncScopedLifestyle.BeginScope(container);
        var bus = scope.GetInstance<IBus>();
        await OperationsSetup.SubscribeEvents(bus,
            container.GetInstance<WorkflowOptions>());
        
        var dispatcher = scope.GetInstance<IOperationDispatcher>();
        var operationManager = scope.GetInstance<IOperationManager>();
        
        var operation = await dispatcher.StartNew<SagaCommand>();
        await operationManager.WaitForOperation(operation!.Id);

        _trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(MultiStepSaga),
                "Initiated",
                typeof(SagaCommand)),
            trace => trace.ShouldMatch(
                typeof(WithoutResponseCommandHandler),
                "Handle",
                typeof(OperationTask<WithoutResponseCommand>)),
            trace => trace.ShouldMatch(
                typeof(MultiStepSaga),
                "Handle",
                typeof(OperationTaskStatusEvent<WithoutResponseCommand>)),
            trace => trace.ShouldMatch(
                typeof(WithResponseCommandHandler),
                "Handle",
                typeof(OperationTask<WithResponseCommand>)),
            trace => trace.ShouldMatch(
                typeof(MultiStepSaga),
                "Handle",
                typeof(OperationTaskStatusEvent<WithResponseCommand>)),
            trace => trace.ShouldMatch(
                typeof(FinalStepCommandHandler),
                "Handle",
                typeof(OperationTask<FinalStepCommand>)),
            trace => trace.ShouldMatch(
                typeof(MultiStepSaga),
                "Handle",
                typeof(OperationTaskStatusEvent<FinalStepCommand>)));

        _testStore.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(o.Id);
                o.Status.Should().Be(OperationStatus.Completed);
                o.Data.Should().BeNull();
            });

        _testStore.AllTasks.Should().HaveCount(4);
        _testStore.AllTasks.Should().AllSatisfy(
            t => t.Status.Should().Be(OperationTaskStatus.Completed));

        _testStore.AllProgress.Should().SatisfyRespectively(
            p => p.Data.Should().Be($"{nameof(WithoutResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithoutResponseCommandHandler)}-2"),
            p => p.Data.Should().Be($"{nameof(WithResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithResponseCommandHandler)}-2"),
            p => p.Data.Should().Be($"{nameof(FinalStepCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(FinalStepCommandHandler)}-2"));
    }
}
