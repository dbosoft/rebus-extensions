using System.Text.Json;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class WorkflowTests : RebusTestBase
{
 
    public WorkflowTests(ITestOutputHelper output)
        : base(output)
    {
    }
    
    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "main")]
    [InlineData(true, "main")]
    public async Task SingleStep_Operation_is_processed(bool sendMode, string eventDestination)
    {
        TestCommandHandler? taskHandler = null;

        using var setup = await SetupRebus(sendMode, eventDestination, configureActivator: (activator, wf, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, new DefaultMessageEnricher()));
            taskHandler = new TestCommandHandler(wf.Messaging);
            activator.Register(() => taskHandler);
            activator.Register(() => new EmptyOperationStatusEventHandler());
            activator.Register(() => new EmptyOperationTaskStatusEventHandler<TestCommand>());
        });
        
        TestOperationManager.Reset();
        TestTaskManager.Reset();
        
        await setup.OperationDispatcher.StartNew<TestCommand>();
        await Task.Delay(1000);
        Assert.True(taskHandler!.Called);
        Assert.Single(TestOperationManager.Operations);
        Assert.Equal(OperationStatus.Completed ,TestOperationManager.Operations.First().Value.Status);

    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "main")]
    [InlineData(true, "main")]
    public async Task MultiStep_Operation_is_processed(bool sendMode, string eventDestination)
    {
        StepOneCommandHandler? stepOneHandler;
        StepTwoCommandHandler? stepTwoHandler;
        using var setup = await SetupRebus(sendMode, eventDestination, configureActivator: (activator, wf, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<MultiStepCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<MultiStepCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepOneCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepOneCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepTwoCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepTwoCommand>>.Instance, new DefaultMessageEnricher()));

            activator.Register(() => new EmptyOperationStatusEventHandler());
            activator.Register(() => new MultiStepSaga(wf));

            stepOneHandler = new StepOneCommandHandler(wf.Messaging);
            stepTwoHandler = new StepTwoCommandHandler(wf.Messaging);
            activator.Register(() => stepOneHandler);
            activator.Register(() => stepTwoHandler);
        });
        
        TestOperationManager.Reset();
        TestTaskManager.Reset();
        StepOneCommandHandler.Called = false;
        StepTwoCommandHandler.Called = false;
        
        await setup.OperationDispatcher.StartNew<MultiStepCommand>();
        await Task.Delay(1000);
        Assert.True(StepOneCommandHandler.Called);
        Assert.True(StepTwoCommandHandler.Called);
        Assert.Single(TestOperationManager.Operations);
        Assert.Equal(3, TestTaskManager.Tasks.Count);
        Assert.Equal(OperationStatus.Completed ,TestOperationManager.Operations.First().Value.Status);

        foreach (var taskModel in TestTaskManager.Tasks)
        {
            Assert.Equal(OperationTaskStatus.Completed, taskModel.Value.Status);
        }
    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "main")]
    [InlineData(true, "main")]
    public async Task Progress_is_reported(bool sendMode, string eventDestination)
    {
        using var setup = await SetupRebus(sendMode, eventDestination, configureActivator: (activator, wf, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, new DefaultMessageEnricher()));

            activator.Register(() => new OperationTaskProgressEventHandler(wf, 
                NullLogger<OperationTaskProgressEventHandler>.Instance));

            activator.Register(() => new EmptyOperationTaskStatusEventHandler<TestCommand>());
            activator.Register(() => new TestCommandHandlerWithProgress(wf.Messaging));
        });
        
        TestOperationManager.Reset();
        TestTaskManager.Reset();
        
        await setup.OperationDispatcher.StartNew<TestCommand>();
        await Task.Delay(1000);
        Assert.Single(TestOperationManager.Progress);
        var progressData = (JsonElement) TestOperationManager.Progress
            .First().Value.First();
        Assert.Equal("progressData",progressData.GetString() );
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SingleStep_Operation_failure_is_reported(bool throws)
    {
  
        using var setup = await SetupRebus(false, "", configureActivator: (activator, wf, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new TestCommandHandlerWithError(throws, wf.Messaging));
            activator.Register(() => new EmptyOperationStatusEventHandler());
            activator.Register(() => new EmptyOperationTaskStatusEventHandler<TestCommand>());
            activator.Register(() =>
                new FailedOperationHandler<OperationTask<TestCommand>>(
                    NullLogger<FailedOperationHandler<OperationTask<TestCommand>>>.Instance,
                    wf.Messaging));
        });
        TestOperationManager.Reset();
        TestTaskManager.Reset();
        
        await setup.OperationDispatcher.StartNew<TestCommand>();
        await Task.Delay(throws ? 2000: 1000);
        Assert.Equal(OperationStatus.Failed ,TestOperationManager.Operations.First().Value.Status);

    }
}