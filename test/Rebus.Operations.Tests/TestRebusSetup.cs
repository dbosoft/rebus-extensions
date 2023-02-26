using Rebus.Bus;

namespace Dbosoft.Rebus.Operations.Tests;

public record TestRebusSetup(IBus Bus, IOperationDispatcher OperationDispatcher, TestOperationManager OperationManager, TestTaskManager TaskManager) : IDisposable
{
    public void Dispose()
    {
        Bus.Dispose();
    }
}