using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.OperationsDB.Tests.Commands;
using JetBrains.Annotations;
using Rebus.Handlers;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

[UsedImplicitly]
public class SimpleCommandHandler : IHandleMessages<OperationTask<SimpleCommand>>
{
    private readonly ITaskMessaging _messaging;

    public SimpleCommandHandler(ITaskMessaging messaging)
    {
        _messaging = messaging;
    }


    public async Task Handle(OperationTask<SimpleCommand> message)
    {
        await Task.Delay(1).ConfigureAwait(false);
        await _messaging.CompleteTask(message).ConfigureAwait(false);
    }
}