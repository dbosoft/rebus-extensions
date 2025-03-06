using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Tests.Handlers;
using Dbosoft.Rebus.Operations.Workflow;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class DispatchAndTypeBasedRoutingWorkflowTest(
    ITestOutputHelper output)
    : WorkflowTests2(output, WorkflowEventDispatchMode.Publish, "main")
{
}

public class DispatchAndExplicitRoutingWorkflowTest(
    ITestOutputHelper output)
    : WorkflowTests2(output, WorkflowEventDispatchMode.Publish, "")
{
}

public class SendAndTypeBasedRoutingWorkflowTest(
    ITestOutputHelper output)
    : WorkflowTests2(output, WorkflowEventDispatchMode.Send, "main")
{
}

public class SendAndExplicitRoutingWorkflowTest(
    ITestOutputHelper output)
    : WorkflowTests2(output, WorkflowEventDispatchMode.Send, "")
{
}

public abstract class WorkflowTests2(
    ITestOutputHelper output,
    WorkflowEventDispatchMode dispatchMode,
    string eventDestination)
    : RebusTestBase2(output, dispatchMode, eventDestination)
{
    [Fact]
    public async Task MultiStep_Operation_is_processed()
    {
        AddTaskHandler<StepWithoutResponseCommand, StepWithoutResponseCommandHandler>();
        AddTaskHandler<StepWithResponseCommand, StepWithResponseCommandHandler>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<MultiStepCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<MultiStepCommand>();
        Assert.NotNull(operation);
        await WaitForOperation(operation!.Id);

        Tracer.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(typeof(MultiStepSaga), "Initiated", typeof(MultiStepCommand)),
            trace => trace.ShouldMatch(typeof(StepWithoutResponseCommandHandler), "Handle", typeof(OperationTask<StepWithoutResponseCommand>)),
            trace => trace.ShouldMatch(typeof(MultiStepSaga), "Handle", typeof(OperationTaskStatusEvent<StepWithoutResponseCommand>)),
            trace => trace.ShouldMatch(typeof(StepWithResponseCommandHandler), "Handle", typeof(OperationTask<StepWithResponseCommand>)),
            trace => trace.ShouldMatch(typeof(MultiStepSaga), "Handle", typeof(OperationTaskStatusEvent<StepWithResponseCommand>)),
            trace => trace.ShouldMatch(typeof(FinalStepCommandHandler), "Handle", typeof(OperationTask<FinalStepCommand>)),
            trace => trace.ShouldMatch(typeof(MultiStepSaga), "Handle", typeof(OperationTaskStatusEvent<FinalStepCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            operationModel =>
            {
                operationModel.Id.Should().Be(operation.Id);
                operationModel.Status.Should().Be(OperationStatus.Completed);
            });

        Store.AllTasks.Should().HaveCount(4)
            .And.AllSatisfy(t => t.Status.Should().Be(OperationTaskStatus.Completed));
    }

    [Fact]
    public async Task MultiStep_CommandWithoutResponseFailsWithError()
    {
        AddTaskHandler<StepWithoutResponseCommand, FailWithErrorHandler<StepWithoutResponseCommand>>();
        AddTaskHandler<StepWithResponseCommand, StepWithResponseCommandHandler>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<MultiStepCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<MultiStepCommand>();
        Assert.NotNull(operation);
        await WaitForOperation(operation!.Id);

        Tracer.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(typeof(MultiStepSaga), "Initiated", typeof(MultiStepCommand)),
            trace => trace.ShouldMatch(
                typeof(FailWithErrorHandler<StepWithoutResponseCommand>),
                "Handle",
                typeof(OperationTask<StepWithoutResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            operationModel =>
            {
                operationModel.Id.Should().Be(operation.Id);
                operationModel.Status.Should().Be(OperationStatus.Failed);
            });

        Store.AllTasks.Should().HaveCount(2);
            //.And.AllSatisfy(t => t.Status.Should().Be(OperationTaskStatus.Completed));
    }

    [Fact]
    public async Task MultiStep_CommandWithoutResponseFailsWithException()
    {
        AddTaskHandler<StepWithoutResponseCommand, FailWithExceptionHandler<StepWithoutResponseCommand>>();
        AddTaskHandler<StepWithResponseCommand, StepWithResponseCommandHandler>();
        AddTaskHandler<FinalStepCommand, FinalStepCommandHandler>();
        AddSaga<MultiStepCommand, MultiStepSaga, MultiStepSagaData>();

        await StartBus();

        var operation = await StartOperation<MultiStepCommand>();
        Assert.NotNull(operation);
        await WaitForOperation(operation!.Id);

        Tracer.Traces.Should().SatisfyRespectively(
            trace => trace.ShouldMatch(typeof(MultiStepSaga), "Initiated", typeof(MultiStepCommand)),
            trace => trace.ShouldMatch(
                typeof(FailWithExceptionHandler<StepWithoutResponseCommand>),
                "Handle",
                typeof(OperationTask<StepWithoutResponseCommand>)));

        Store.AllOperations.Should().SatisfyRespectively(
            operationModel =>
            {
                operationModel.Id.Should().Be(operation.Id);
                operationModel.Status.Should().Be(OperationStatus.Failed);
            });

        Store.AllTasks.Should().HaveCount(2);
        //.And.AllSatisfy(t => t.Status.Should().Be(OperationTaskStatus.Completed));
    }
}
