using Dbosoft.Rebus.Operations.Workflow;
using Rebus.Bus;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestCommandHandler : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly IBus _bus;
    public bool Called { get; private set; }

    public TestCommandHandler(IBus bus)
    {
        _bus = bus;
    }
    
    public Task Handle(OperationTask<TestCommand> message)
    {
        Called = true;
        return _bus.CompleteTask(message);
    }
}

public class StepTwoCommandHandler : IHandleMessages<OperationTask<StepTwoCommand>>
{
    private readonly IBus _bus;

    public StepTwoCommandHandler(IBus bus)
    {
        _bus = bus;
    }

    public bool Called { get; private set; }

    public Task Handle(OperationTask<StepTwoCommand> message)
    {
        Called = true;
        return _bus.CompleteTask(message);
    }
}

public class StepOneCommandHandler : IHandleMessages<OperationTask<StepOneCommand>>
{
    private readonly IBus _bus;

    public StepOneCommandHandler(IBus bus)
    {
        _bus = bus;
    }

    public bool Called { get; private set; }

    public Task Handle(OperationTask<StepOneCommand> message)
    {
        Called = true;
        return _bus.CompleteTask(message);
    }
}