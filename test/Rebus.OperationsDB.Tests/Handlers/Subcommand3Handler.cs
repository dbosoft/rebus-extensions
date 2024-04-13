using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.OperationsDB.Tests.Commands;
using JetBrains.Annotations;
using Rebus.Handlers;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

[UsedImplicitly]
public class Subcommand3Handler : IHandleMessages<OperationTask<SubCommand3>>
{
    private readonly ITaskMessaging _taskMessaging;

    public Subcommand3Handler(ITaskMessaging taskMessaging)
    {
        _taskMessaging = taskMessaging;
    }

    public async Task Handle(OperationTask<SubCommand3> message)
    {
        await _taskMessaging.ProgressMessage(message, "started task Subcommand3").ConfigureAwait(false);
        await Task.Delay(1).ConfigureAwait(false);
        await _taskMessaging.ProgressMessage(message, 50).ConfigureAwait(false);
        await Task.Delay(1).ConfigureAwait(false);
        await _taskMessaging.ProgressMessage(message, 80).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false);
        await _taskMessaging.CompleteTask(message).ConfigureAwait(false);
    }
}
