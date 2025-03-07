using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Tests.Handlers;
using Dbosoft.Rebus.Operations.Workflow;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class DispatchAndTypeBasedRoutingMessageEnricherTests(
    ITestOutputHelper output)
    : MessageEnricherTests(output, WorkflowEventDispatchMode.Publish, true)
{
}

public class DispatchAndExplicitRoutingMessageEnricherTests(
    ITestOutputHelper output)
    : MessageEnricherTests(output, WorkflowEventDispatchMode.Publish, false)
{
}

public class SendAndTypeBasedRoutingMessageEnricherTests(
    ITestOutputHelper output)
    : MessageEnricherTests(output, WorkflowEventDispatchMode.Send, true)
{
}

public class SendAndExplicitRoutingMessageEnricherTests(
    ITestOutputHelper output)
    : MessageEnricherTests(output, WorkflowEventDispatchMode.Send, false)
{
}

public abstract class MessageEnricherTests(
    ITestOutputHelper output,
    WorkflowEventDispatchMode dispatchMode,
    bool useTypeBasedRouting)
    : RebusTestBase(output, dispatchMode, useTypeBasedRouting, new TestMessageEnricher())
{
    [Fact]
    public async Task Headers_are_passed_to_task()
    {
        AddTaskHandler<UseHeadersCommand, UseHeadersCommandHandler>();

        await StartBus();

        var operation = await StartOperation<UseHeadersCommand>(
            additionalHeaders: new Dictionary<string, string>
            {
                ["custom_header"] = "custom header value"
            });
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace =>
            {
                trace.ShouldMatch(typeof(UseHeadersCommandHandler), "Handle", typeof(OperationTask<UseHeadersCommand>));
                trace.Data.Should().Be("custom header value");
            });

        Store.AllOperations.Should().SatisfyRespectively(
            operationModel =>
            {
                operationModel.Id.Should().Be(operation.Id);
                operationModel.Status.Should().Be(OperationStatus.Completed);
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Completed));
    }

    private sealed class TestMessageEnricher : IMessageEnricher
    {
        public object? EnrichTaskAcceptedReply<T>(
            OperationTaskSystemMessage<T> taskMessage)
            where T : class, new()
        {
            return null;
        }

        public IDictionary<string, string>? EnrichHeadersFromIncomingSystemMessage<T>(
            OperationTaskSystemMessage<T> taskMessage,
            IDictionary<string, string> systemMessageHeaders)
        {
            return CopyCustomHeader(systemMessageHeaders);
        }

        public IDictionary<string, string>? EnrichHeadersOfOutgoingSystemMessage(
            object taskMessage,
            IDictionary<string, string>? previousHeaders)
        {
            return CopyCustomHeader(previousHeaders);
        }

        public IDictionary<string, string>? EnrichHeadersOfStatusEvent(
            OperationStatusEvent operationStatusEvent,
            IDictionary<string, string>? previousHeaders)
        {
            return CopyCustomHeader(previousHeaders);
        }

        public IDictionary<string, string>? EnrichHeadersOfTaskStatusEvent(
            OperationTaskStatusEvent operationStatusEvent,
            IDictionary<string, string>? previousHeaders)
        {
            return CopyCustomHeader(previousHeaders);
        }

        private static IDictionary<string, string>? CopyCustomHeader(
            IDictionary<string, string>? headers)
        {
            return headers?.TryGetValue("custom_header", out var customHeader) == true
                ? new Dictionary<string, string> { { "custom_header", customHeader } }
                : null;
        }
    }
}
