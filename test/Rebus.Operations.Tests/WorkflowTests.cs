using System.Text.Json;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Rebus.Retry.Simple;
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

        using var setup = await SetupRebus(sendMode, eventDestination, configureActivator: (activator,_,tasks, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, new DefaultMessageEnricher()));
            taskHandler = new TestCommandHandler(tasks);
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
        using var setup = await SetupRebus(sendMode, eventDestination, configureActivator: (activator, wf, tasks, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<MultiStepCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<MultiStepCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepOneCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepOneCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepTwoCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepTwoCommand>>.Instance, new DefaultMessageEnricher()));

            activator.Register(() => new EmptyOperationStatusEventHandler());
            activator.Register(() => new MultiStepSaga(wf));

            stepOneHandler = new StepOneCommandHandler(tasks);
            stepTwoHandler = new StepTwoCommandHandler(tasks);
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
    public async Task MultiStep_Operation_Exception_is_reported(bool sendMode, string eventDestination)
    {
        StepOneCommandHandler? stepOneHandler;
        StepTwoCommandHandler? stepTwoHandler;
        using var setup = await SetupRebus(sendMode, eventDestination, configureActivator: (activator, wf, tasks, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<MultiStepCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<MultiStepCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepOneCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepOneCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new IncomingTaskMessageHandler<StepTwoCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<StepTwoCommand>>.Instance, new DefaultMessageEnricher()));

            activator.Register(() => new EmptyOperationStatusEventHandler());
            activator.Register(() => new MultiStepSaga(wf));
            activator.Register(() => new FailedOperationHandler<OperationTask<StepTwoCommand>>(wf.WorkflowOptions,
                NullLogger< FailedOperationHandler<OperationTask<StepTwoCommand>>>.Instance,
                wf.Messaging));

            stepOneHandler = new StepOneCommandHandler(tasks);
            stepTwoHandler = new StepTwoCommandHandler(tasks){Throws = true};
            activator.Register(() => stepOneHandler);
            activator.Register(() => stepTwoHandler);
        });

        TestOperationManager.Reset();
        TestTaskManager.Reset();
        StepOneCommandHandler.Called = false;
        StepTwoCommandHandler.Called = false;

        await setup.OperationDispatcher.StartNew<MultiStepCommand>();

        var timeout = new CancellationTokenSource(60000);
        while (
                !timeout.Token.IsCancellationRequested &&
                (TestOperationManager.Operations.First().Value.Status == OperationStatus.Running ||
                 TestOperationManager.Operations.First().Value.Status == OperationStatus.Queued))
            // ReSharper disable once MethodSupportsCancellation
        {
            await Task.Delay(1000);
        }

        Assert.True(StepOneCommandHandler.Called);
        Assert.True(StepTwoCommandHandler.Called);
        Assert.Single(TestOperationManager.Operations);
        Assert.Equal(3, TestTaskManager.Tasks.Count);
        Assert.Equal(OperationStatus.Failed, TestOperationManager.Operations.First().Value.Status);


    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "main")]
    [InlineData(true, "main")]
    public async Task Progress_is_reported(bool sendMode, string eventDestination)
    {
        using var setup = await SetupRebus(sendMode, eventDestination, configureActivator: (activator, wf,tasks, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, new DefaultMessageEnricher()));

            activator.Register(() => new OperationTaskProgressEventHandler(wf, 
                NullLogger<OperationTaskProgressEventHandler>.Instance));

            activator.Register(() => new EmptyOperationTaskStatusEventHandler<TestCommand>());
            activator.Register(() => new TestCommandHandlerWithProgress(tasks));
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
  
        using var setup = await SetupRebus(false, "", configureActivator: (activator, wf,tasks, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, new DefaultMessageEnricher()));
            activator.Register(() => new TestCommandHandlerWithError(tasks){Throws = true});
            activator.Register(() => new EmptyOperationStatusEventHandler());
            activator.Register(() => new EmptyOperationTaskStatusEventHandler<TestCommand>());
            activator.Register(() =>
                new FailedOperationHandler<OperationTask<TestCommand>>(
                    wf.WorkflowOptions,
                    NullLogger<FailedOperationHandler<OperationTask<TestCommand>>>.Instance,
                    wf.Messaging));
        });
        TestOperationManager.Reset();
        TestTaskManager.Reset();
        
        await setup.OperationDispatcher.StartNew<TestCommand>();
        var timeout = new CancellationTokenSource(10000);
        while (
                !timeout.Token.IsCancellationRequested &&
                (TestOperationManager.Operations.First().Value.Status == OperationStatus.Running ||
                 TestOperationManager.Operations.First().Value.Status == OperationStatus.Queued))
            // ReSharper disable once MethodSupportsCancellation
        {
            await Task.Delay(1000);
        }
        Assert.Equal(OperationStatus.Failed ,TestOperationManager.Operations.First().Value.Status);

    }

    [Fact]
    public async Task Headers_are_passed_to_task()
    {
        var messageEnricher = new TestMessageEnricher();
        using var setup = await SetupRebus(false, "", configureActivator: (activator, wf,tasks, bus) =>
        {
            activator.Register(() => new IncomingTaskMessageHandler<TestCommand>(bus,
                NullLogger<IncomingTaskMessageHandler<TestCommand>>.Instance, messageEnricher));
            activator.Register(() => new ExposingHeadersCommandHandler(tasks));
            activator.Register(() => new EmptyOperationStatusEventHandler());
            activator.Register(() => new EmptyOperationTaskStatusEventHandler<TestCommand>());
            activator.Register(() =>
                new FailedOperationHandler<OperationTask<TestCommand>>(
                    wf.WorkflowOptions,
                    NullLogger<FailedOperationHandler<OperationTask<TestCommand>>>.Instance,
                    wf.Messaging));
        }, messageEnricher);
        TestOperationManager.Reset();
        TestTaskManager.Reset();
        
        await setup.OperationDispatcher.StartNew<TestCommand>(additionalHeaders: 
            new Dictionary<string, string>{{"custom_header", "data"}});
        await Task.Delay(1000);
        Assert.True(ExposingHeadersCommandHandler.Called);
        var headers = ExposingHeadersCommandHandler.Headers;
        Assert.NotNull(headers);
        Assert.Contains(headers, x => x.Key == "custom_header");
        
    }

    private class TestMessageEnricher : IMessageEnricher
    {
        public object? EnrichTaskAcceptedReply<T>(OperationTaskSystemMessage<T> taskMessage) where T : class, new()
        {
            return null;
        }

        private static IDictionary<string, string>? CopyCustomHeader(IDictionary<string, string>? headers)
        {
            if (headers == null || !headers.ContainsKey("custom_header"))
                return null;

            return new Dictionary<string, string> { { "custom_header", headers["custom_header"] } };
        }
        
        public IDictionary<string, string>? EnrichHeadersFromIncomingSystemMessage<T>(OperationTaskSystemMessage<T> taskMessage,
            IDictionary<string, string> systemMessageHeaders)
        {
            return CopyCustomHeader(systemMessageHeaders);
        }

        public IDictionary<string, string>? EnrichHeadersOfOutgoingSystemMessage(object taskMessage, IDictionary<string, string>? previousHeaders)
        {
            return CopyCustomHeader(previousHeaders);
        }

        public IDictionary<string, string>? EnrichHeadersOfStatusEvent(OperationStatusEvent operationStatusEvent, IDictionary<string, string>? previousHeaders)
        {
            return CopyCustomHeader(previousHeaders);
        }

        public IDictionary<string, string>? EnrichHeadersOfTaskStatusEvent(OperationTaskStatusEvent operationStatusEvent,
            IDictionary<string, string>? previousHeaders)
        {
            return CopyCustomHeader(previousHeaders);
        }
    }
}