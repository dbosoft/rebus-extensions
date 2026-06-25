using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit;

namespace Dbosoft.Rebus.Operations.Tests;

// Bottom-up isolation of the cancellation building blocks, independent of the
// full operation saga and of any blocking task handler.
public class CancellationUnitTests
{
    [Fact]
    public void Registry_cancel_trips_the_registered_token()
    {
        var registry = new TaskCancellationRegistry();
        var operationId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var token = registry.Register(operationId, taskId);
        token.IsCancellationRequested.Should().BeFalse();

        // A second registration is idempotent.
        registry.Register(operationId, taskId).Should().Be(token);

        registry.Cancel(operationId);
        token.IsCancellationRequested.Should().BeTrue();

        // Remove disposes the source and is safe to call once.
        registry.Remove(operationId, taskId);
    }

    [Fact]
    public void Registry_cancel_for_other_operation_does_not_trip_token()
    {
        var registry = new TaskCancellationRegistry();
        var operationId = Guid.NewGuid();
        var token = registry.Register(operationId, Guid.NewGuid());

        registry.Cancel(Guid.NewGuid());

        token.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void Registry_is_safe_to_cancel_and_remove_after_removal()
    {
        var registry = new TaskCancellationRegistry();
        var operationId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        registry.Register(operationId, taskId);
        registry.IsCancellationRequested(operationId, taskId).Should().BeFalse();

        registry.Remove(operationId, taskId);

        // After removal these are all no-ops and must not throw.
        registry.Cancel(operationId);
        registry.Remove(operationId, taskId);
        registry.IsCancellationRequested(operationId, taskId).Should().BeFalse();
    }

    [Fact]
    public void Registry_reports_cancellation_only_while_registered_and_cancelled()
    {
        var registry = new TaskCancellationRegistry();
        var operationId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        registry.Register(operationId, taskId);
        registry.IsCancellationRequested(operationId, taskId).Should().BeFalse();

        registry.Cancel(operationId);
        registry.IsCancellationRequested(operationId, taskId).Should().BeTrue();

        registry.Remove(operationId, taskId);
        registry.IsCancellationRequested(operationId, taskId).Should().BeFalse();
    }

    [Fact]
    public async Task Broadcast_event_trips_a_registered_token_in_this_process()
    {
        var registry = new TaskCancellationRegistry();
        var operationId = Guid.NewGuid();
        var token = registry.Register(operationId, Guid.NewGuid());

        using var activator = new BuiltinHandlerActivator();
        activator.Register(() => new OperationCancellationRequestedHandler(
            registry, NullLogger<OperationCancellationRequestedHandler>.Instance));

        var network = new InMemNetwork();
        var starter = Configure.With(activator)
            .Transport(t => t.UseInMemoryTransport(network, "main"))
            .Routing(r => r.TypeBased().Map<OperationCancellationRequestedEvent>("main"))
            .Sagas(s => s.StoreInMemory())
            .Create();
        var bus = starter.Bus;
        await bus.Subscribe<OperationCancellationRequestedEvent>();
        starter.Start();

        await bus.Publish(new OperationCancellationRequestedEvent { OperationId = operationId });

        // Wait briefly for the broadcast to be handled.
        var start = DateTimeOffset.UtcNow;
        while (!token.IsCancellationRequested && DateTimeOffset.UtcNow - start < TimeSpan.FromSeconds(5))
            await Task.Delay(50);

        token.IsCancellationRequested.Should().BeTrue();
    }
}
