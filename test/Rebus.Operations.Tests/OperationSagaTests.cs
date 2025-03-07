using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Tests.Handlers;
using Dbosoft.Rebus.Operations.Tests.Sagas;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class DispatchAndTypeBasedRoutingOperationSagaTests(
    ITestOutputHelper output)
    : OperationSagaTests(output, WorkflowEventDispatchMode.Publish, true)
{
}

public class DispatchAndExplicitRoutingOperationSagaTests(
    ITestOutputHelper output)
    : OperationSagaTests(output, WorkflowEventDispatchMode.Publish, false)
{
}

public class SendAndTypeBasedRoutingOperationSagaTests(
    ITestOutputHelper output)
    : OperationSagaTests(output, WorkflowEventDispatchMode.Send, true)
{
}

public class SendAndExplicitRoutingOperationSagaTests(
    ITestOutputHelper output)
    : OperationSagaTests(output, WorkflowEventDispatchMode.Send, false)
{
}

public abstract class OperationSagaTests(
    ITestOutputHelper output,
    WorkflowEventDispatchMode dispatchMode,
    bool useTypeBasedRouting)
    : RebusTestBase(output, dispatchMode, useTypeBasedRouting)
{
    [Fact]
    public async Task Operation_is_processed()
    {
        AddTaskHandler<WithoutResponseCommand, WithoutResponseCommandHandler>();
        AddTaskHandler<WithResponseCommand, WithResponseCommandHandler>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<SagaCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
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
            trace =>
            {
                trace.ShouldMatch(
                    typeof(MultiStepSaga),
                    "Handle",
                    typeof(OperationTaskStatusEvent<WithResponseCommand>));
                trace.Data.Should().BeOfType<WithResponseCommandResponse>()
                    .Which.Data.Should().Be("test");
            },
            trace => trace.ShouldMatch(
                typeof(FinalStepCommandHandler),
                "Handle",
                typeof(OperationTask<FinalStepCommand>)),
            trace => trace.ShouldMatch(
                typeof(MultiStepSaga),
                "Handle",
                typeof(OperationTaskStatusEvent<FinalStepCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Completed);
                o.Data.Should().BeNull();
            });

        Store.AllTasks.Should().HaveCount(4);
        Store.AllTasks.Should().AllSatisfy(t => t.Status.Should().Be(OperationTaskStatus.Completed));

        Store.AllProgress.Should().SatisfyRespectively(
            p => p.Data.Should().BeOfType<string>().Which.Should().Be($"{nameof(WithoutResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithoutResponseCommandHandler)}-2"),
            p => p.Data.Should().Be($"{nameof(WithResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithResponseCommandHandler)}-2"),
            p => p.Data.Should().Be($"{nameof(FinalStepCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(FinalStepCommandHandler)}-2"));
    }

    [Fact]
    public async Task Parallel_tasks_are_processed()
    {
        AddTaskHandler<WithoutResponseCommand, WithoutResponseCommandHandler>();
        AddSaga<SagaCommand, ParallelSaga, ParallelSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().HaveCount(7);

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Completed);
                o.Data.Should().BeNull();
            });

        Store.AllTasks.Should().HaveCount(4);
        Store.AllTasks.Should().AllSatisfy(t => t.Status.Should().Be(OperationTaskStatus.Completed));
    }

    [Fact]
    public async Task Command_without_response_fails_with_error()
    {
        AddTaskHandler<WithoutResponseCommand, FailWithErrorHandler<WithoutResponseCommand>>();
        AddTaskHandler<WithResponseCommand, WithResponseCommandHandler>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<SagaCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(MultiStepSaga),
                "Initiated",
                typeof(SagaCommand)),
            trace => trace.ShouldMatch(
                typeof(FailWithErrorHandler<WithoutResponseCommand>),
                "Handle",
                typeof(OperationTask<WithoutResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<ErrorData>().Which.ErrorMessage.Should().Be("TEST ERROR!");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Failed),
            t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().BeEmpty();
    }

    [Fact]
    public async Task Command_without_response_fails_with_exception()
    {
        AddTaskHandler<WithoutResponseCommand, FailWithExceptionHandler<WithoutResponseCommand>>();
        AddTaskHandler<WithResponseCommand, WithResponseCommandHandler>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<SagaCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(MultiStepSaga),
                "Initiated",
                typeof(SagaCommand)),
            trace => trace.ShouldMatch(
                typeof(FailWithExceptionHandler<WithoutResponseCommand>),
                "Handle",
                typeof(OperationTask<WithoutResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<ErrorData>().Which.ErrorMessage.Should().Match("*TEST EXCEPTION!*");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Failed),
            t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().BeEmpty();
    }

    [Fact]
    public async Task Command_with_response_fails_with_error()
    {
        AddTaskHandler<WithoutResponseCommand, WithoutResponseCommandHandler>();
        AddTaskHandler<WithResponseCommand, FailWithErrorHandler<WithResponseCommand>>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<SagaCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
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
                typeof(FailWithErrorHandler<WithResponseCommand>),
                "Handle",
                typeof(OperationTask<WithResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<ErrorData>().Which.ErrorMessage.Should().Be("TEST ERROR!");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Failed),
            t => t.Status.Should().Be(OperationTaskStatus.Completed),
            t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().SatisfyRespectively(
            p => p.Data.Should().BeOfType<string>().Which.Should().Be($"{nameof(WithoutResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithoutResponseCommandHandler)}-2"));
    }

    [Fact]
    public async Task Command_with_response_fails_with_exception()
    {
        AddTaskHandler<WithoutResponseCommand, WithoutResponseCommandHandler>();
        AddTaskHandler<WithResponseCommand, FailWithExceptionHandler<WithResponseCommand>>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<SagaCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
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
                typeof(FailWithExceptionHandler<WithResponseCommand>),
                "Handle",
                typeof(OperationTask<WithResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<ErrorData>().Which.ErrorMessage.Should().Match("*TEST EXCEPTION!*");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Failed),
            t => t.Status.Should().Be(OperationTaskStatus.Completed),
            t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().SatisfyRespectively(
            p => p.Data.Should().BeOfType<string>().Which.Should().Be($"{nameof(WithoutResponseCommandHandler)}-1"),
            p => p.Data.Should().Be($"{nameof(WithoutResponseCommandHandler)}-2"));
    }

    [Fact]
    public async Task Saga_fails_initialization_with_error()
    {
        AddSaga<SagaCommand, FailWithErrorSaga, FailWithErrorSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(FailWithErrorSaga),
                "Initiated",
                typeof(SagaCommand)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<string>().Which.Should().Be("TEST ERROR!");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().BeEmpty();
    }

    [Fact]
    public async Task Saga_fails_initialization_with_exception()
    {
        AddSaga<SagaCommand, FailWithExceptionSaga, FailWithExceptionSagaData>();

        await StartBus();

        var operation = await StartOperation<SagaCommand>();
        await WaitForOperation(operation!.Id);

        Trace.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(
                typeof(FailWithExceptionSaga),
                "Initiated",
                typeof(SagaCommand)));

        Store.AllOperations.Should().SatisfyRespectively(
            o =>
            {
                o.Id.Should().Be(operation.Id);
                o.Status.Should().Be(OperationStatus.Failed);
                o.Data.Should().BeOfType<ErrorData>().Which.ErrorMessage.Should().Match("*TEST EXCEPTION!*");
            });

        Store.AllTasks.Should().SatisfyRespectively(
            t => t.Status.Should().Be(OperationTaskStatus.Failed));

        Store.AllProgress.Should().BeEmpty();
    }
}
