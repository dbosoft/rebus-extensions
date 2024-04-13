using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.OperationsDB.Tests.Commands;
using JetBrains.Annotations;
using Rebus.Handlers;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

[UsedImplicitly]
public class Subcommand1Handler : IHandleMessages<OperationTask<SubCommand1>>
{
    private readonly ITaskMessaging _taskMessaging;

    public Subcommand1Handler(ITaskMessaging taskMessaging)
    {
        _taskMessaging = taskMessaging;
    }

    public async Task Handle(OperationTask<SubCommand1> message)
    {
        await _taskMessaging.ProgressMessage(message, "started task Subcommand1").ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);
        await _taskMessaging.CompleteTask(message).ConfigureAwait(false);
    }
}