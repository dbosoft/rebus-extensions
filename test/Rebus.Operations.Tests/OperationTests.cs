using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Tests.Handlers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class PublishAndTypeBasedRoutingOperationTests(
    ITestOutputHelper output)
    : OperationTests(output, WorkflowEventDispatchMode.Publish, true)
{
}

public class PublishAndExplicitRoutingOperationTests(
    ITestOutputHelper output)
    : OperationTests(output, WorkflowEventDispatchMode.Publish, false)
{
}

public class SendAndTypeBasedRoutingOperationTests(
    ITestOutputHelper output)
    : OperationTests(output, WorkflowEventDispatchMode.Send, true)
{
}

public class SendAndExplicitRoutingOperationTests(
    ITestOutputHelper output)
    : OperationTests(output, WorkflowEventDispatchMode.Send, false)
{
}

public abstract class OperationTests(
    ITestOutputHelper output,
    WorkflowEventDispatchMode dispatchMode,
    bool useTypeBasedRouting)
    : RebusTestBase(output, dispatchMode, useTypeBasedRouting)
{
    [Fact]
    public async Task Command_without_response_is_processed_and_progress_is_reported()
    {
        AddTaskHandler<WithoutResponseCommand, WithoutResponseCommandHandler>();

        await StartBus();

        var operation = await StartOperation<WithoutResponseCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(WithoutResponseCommandHandler),
                "Handle",
                typeof(OperationTask<WithoutResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Completed);
                o.Data.Should().BeNull();
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Completed));

        Store.AllProgress.Should().SatisfyRespectively(
            p => p.Data.Should().Be($"{nameof(WithoutResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithoutResponseCommandHandler)}-2"));
    }

    [Fact]
    public async Task Command_with_response_is_processed_and_progress_is_reported()
    {
        AddTaskHandler<WithResponseCommand, WithResponseCommandHandler>();

        await StartBus();

        var operation = await StartOperation<WithResponseCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(WithResponseCommandHandler),
                "Handle",
                typeof(OperationTask<WithResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Completed);
                o.Data.Should().BeOfType<WithResponseCommandResponse>()
                    .Which.Data.Should().Be("test");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Completed));

        Store.AllProgress.Should().SatisfyRespectively(
            p => p.Data.Should().Be($"{nameof(WithResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithResponseCommandHandler)}-2"));
    }

    [Fact]
    public async Task Error_is_reported()
    {
        AddTaskHandler<WithoutResponseCommand, FailWithErrorHandler<WithoutResponseCommand>>();

        await StartBus();

        var operation = await StartOperation<WithoutResponseCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(FailWithErrorHandler<WithoutResponseCommand>),
                "Handle",
                typeof(OperationTask<WithoutResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<ErrorData>()
                    .Which.ErrorMessage.Should().Be("TEST ERROR!");
            });

        Store.AllTasks.Should().SatisfyRespectively(
                t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().BeEmpty();
    }

    [Fact]
    public async Task Exception_is_reported()
    {
        AddTaskHandler<WithoutResponseCommand, FailWithExceptionHandler<WithoutResponseCommand>>();

        await StartBus();

        var operation = await StartOperation<WithoutResponseCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(FailWithExceptionHandler<WithoutResponseCommand>),
                "Handle",
                typeof(OperationTask<WithoutResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<ErrorData>()
                    .Which.ErrorMessage.Should().Match("*TEST EXCEPTION!*");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().BeEmpty();
    }
}
