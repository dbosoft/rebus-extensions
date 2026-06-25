using System.Text.Json;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Dbosoft.Rebus.Operations.Tests.Handlers;
using Dbosoft.Rebus.Operations.Tests.Sagas;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

// A single concrete class (methods run sequentially) so the handlers' static
// coordination state is not clobbered by parallel test classes. Cancellation
// behaviour is independent of the dispatch/routing mode, so one representative
// combination (publish + type-based routing) is used.
public class CancellationTests(ITestOutputHelper output)
    : RebusTestBase(output, WorkflowEventDispatchMode.Publish, true)
{
    [Fact]
    public async Task Cancellation_cancels_a_running_opted_in_task()
    {
        CancellableCommandHandler.Reset();
        AddTaskHandler<CancellableCommand, CancellableCommandHandler>();
        await StartBus();

        var operation = await OperationDispatcher.StartNew<CancellableCommand>();

        // Wait until the handler has registered its cancellation token and is blocked.
        await CancellableCommandHandler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await OperationDispatcher.RequestCancellation(operation.Id);
        await WaitForOperation(operation.Id);

        CancellableCommandHandler.TokenObserved.Should().BeTrue();

        Store.AllOperations.Should().ContainSingle()
            .Which.Status.Should().Be(OperationStatus.Cancelled);
        Store.AllTasks.Should().ContainSingle()
            .Which.Status.Should().Be(OperationTaskStatus.Cancelled);
    }

    [Fact]
    public async Task Cancellation_does_not_affect_a_handler_that_did_not_opt_in()
    {
        IgnoreCancellationCommandHandler.Reset();
        AddTaskHandler<IgnoreCancellationCommand, IgnoreCancellationCommandHandler>();
        await StartBus();

        var operation = await OperationDispatcher.StartNew<IgnoreCancellationCommand>();
        await IgnoreCancellationCommandHandler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // The request is broadcast but the handler never asked for a token, so it is a no-op.
        await OperationDispatcher.RequestCancellation(operation.Id);
        IgnoreCancellationCommandHandler.Release.TrySetResult(true);

        await WaitForOperation(operation.Id);

        Store.AllOperations.Should().ContainSingle()
            .Which.Status.Should().Be(OperationStatus.Completed);
        Store.AllTasks.Should().ContainSingle()
            .Which.Status.Should().Be(OperationTaskStatus.Completed);
    }

    [Fact]
    public async Task RequestCancellation_for_unknown_operation_is_a_noop()
    {
        await StartBus();

        await OperationDispatcher.RequestCancellation(Guid.NewGuid());

        // Give the message time to be (not) handled; nothing should be created or thrown.
        await Task.Delay(300);
        Store.AllOperations.Should().BeEmpty();
    }

    [Fact]
    public async Task Unrelated_OperationCanceledException_is_not_reported_as_cancelled()
    {
        AddTaskHandler<SelfCancelCommand, SelfCancelCommandHandler>();
        await StartBus();

        var operation = await OperationDispatcher.StartNew<SelfCancelCommand>();
        await WaitForOperation(operation.Id);

        // The handler's own cancellation must surface as a failure, never as Cancelled.
        Store.AllOperations.Should().ContainSingle()
            .Which.Status.Should().Be(OperationStatus.Failed);
    }

    [Fact]
    public async Task Opted_in_handler_that_fails_with_an_ordinary_exception_ends_failed()
    {
        AddTaskHandler<OptInThenFailCommand, OptInThenFailCommandHandler>();
        await StartBus();

        var operation = await OperationDispatcher.StartNew<OptInThenFailCommand>();
        await WaitForOperation(operation.Id);

        // The opt-in registration is cleaned up on the failure path (see OperationCancellationStep);
        // the task surfaces as a normal failure.
        Store.AllOperations.Should().ContainSingle()
            .Which.Status.Should().Be(OperationStatus.Failed);
    }

    [Fact]
    public async Task Cancellation_propagates_through_a_saga_task_tree()
    {
        CancellableCommandHandler.Reset();
        AddTaskHandler<CancellableCommand, CancellableCommandHandler>();
        AddSaga<CancellableSagaCommand, CancellableSaga, CancellableSagaData>();
        await StartBus();

        var operation = await OperationDispatcher.StartNew<CancellableSagaCommand>();
        await CancellableCommandHandler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await OperationDispatcher.RequestCancellation(operation.Id);
        await WaitForOperation(operation.Id);

        // The leaf task is cancelled and the cancellation propagates up to the operation.
        Store.AllOperations.Should().ContainSingle()
            .Which.Status.Should().Be(OperationStatus.Cancelled);
        Store.AllTasks.Should().Contain(t => t.Status == OperationTaskStatus.Cancelled);
    }
}

// No bus required: verifies the new cancelled flag is wire-compatible.
public class OperationTaskStatusEventCompatibilityTests
{
    [Fact]
    public void Status_event_without_cancelled_flag_deserializes_as_not_cancelled()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json =
            $$"""
              {
                "OperationFailed": false,
                "OperationId": "{{Guid.NewGuid()}}",
                "InitiatingTaskId": "{{Guid.NewGuid()}}",
                "TaskId": "{{Guid.NewGuid()}}",
                "Created": "2026-01-01T00:00:00+00:00"
              }
              """;

        var statusEvent = JsonSerializer.Deserialize<OperationTaskStatusEvent>(json, options);

        statusEvent.Should().NotBeNull();
        statusEvent!.OperationCancelled.Should().BeFalse();
        statusEvent.OperationFailed.Should().BeFalse();
    }

    [Fact]
    public void Cancelled_factory_sets_only_the_cancelled_flag()
    {
        var statusEvent = OperationTaskStatusEvent.Cancelled(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        statusEvent.OperationCancelled.Should().BeTrue();
        statusEvent.OperationFailed.Should().BeFalse();
    }
}
