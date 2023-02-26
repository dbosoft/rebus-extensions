using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Tests;
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

    public SimpleInjectorWorkflowTests(ITestOutputHelper output)
        : base(output)
    {
    }


    [Fact]
    public async Task MultiStep_Operation_is_processed()
    {
        var sc = new ServiceCollection();
        sc.AddLogging();
        
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        sc.AddSimpleInjector(container, cfg => cfg.AddLogging());

        SetupRebus(container);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(MultiStepSaga));
        container.Collection.Append(typeof(IHandleMessages<>), typeof(StepOneCommandHandler), Lifestyle.Scoped);
        container.Collection.Append(typeof(IHandleMessages<>), typeof(StepTwoCommandHandler), Lifestyle.Scoped);

        container.Register<TestOperationManager>(Lifestyle.Scoped);
        container.Register<TestTaskManager>(Lifestyle.Scoped);
        container.Register<StepOneCommandHandler>(Lifestyle.Scoped);
        container.Register<StepTwoCommandHandler>(Lifestyle.Scoped);
        
        var sp = sc.BuildServiceProvider();
        sp.UseSimpleInjector(container);
        
        container.Verify();

        TestOperationManager.Reset();
        TestTaskManager.Reset();
        StepOneCommandHandler.Called = false;
        StepTwoCommandHandler.Called = false;

        await using var scope = AsyncScopedLifestyle.BeginScope(container);
        var bus = scope.GetInstance<IBus>();
        await OperationsSetup.SubscribeEvents(bus);
        
        var dispatcher = scope.GetInstance<IOperationDispatcher>();

        await dispatcher.StartNew<MultiStepCommand>();
        await Task.Delay(1000);
        Assert.True(StepOneCommandHandler.Called);
        Assert.True(StepTwoCommandHandler.Called);
        
        Assert.Single(TestOperationManager.Operations);
        Assert.Equal(3, TestTaskManager.Tasks.Count);
        Assert.Equal(OperationStatus.Completed, TestOperationManager.Operations.First().Value.Status);

        foreach (var taskModel in TestTaskManager.Tasks)
        {
            Assert.Equal(OperationTaskStatus.Completed, taskModel.Value.Status);
        }
    }

}