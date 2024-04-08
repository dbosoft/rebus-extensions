using Dbosoft.Rebus.Operations;
using JetBrains.Annotations;
using Rebus.Handlers;

namespace Dbosoft.Rebus.OperationsDB.Tests;

[UsedImplicitly]
public class SimpleCommandHandler : IHandleMessages<OperationTask<SimpleCommand>>
{
    private readonly ITaskMessaging _messaging;

    public SimpleCommandHandler(ITaskMessaging messaging)
    {
        _messaging = messaging;
    }


    public Task Handle(OperationTask<SimpleCommand> message)
    {
        return _messaging.CompleteTask(message);
    }
}