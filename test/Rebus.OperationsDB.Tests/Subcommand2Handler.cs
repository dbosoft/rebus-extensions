using Dbosoft.Rebus.Operations;
using Rebus.Handlers;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class Subcommand2Handler : IHandleMessages<OperationTask<SubCommand2>>
{
    private readonly ITaskMessaging _taskMessaging;

    public Subcommand2Handler(ITaskMessaging taskMessaging)
    {
        _taskMessaging = taskMessaging;
    }
    
    public async Task Handle(OperationTask<SubCommand2> message)
    {
        await Task.Delay(1).ConfigureAwait(false);
        await _taskMessaging.ProgressMessage(message, "started task Subcommand2").ConfigureAwait(false);
        await Task.Delay(1).ConfigureAwait(false);
        await _taskMessaging.CompleteTask(message).ConfigureAwait(false);
    }
}