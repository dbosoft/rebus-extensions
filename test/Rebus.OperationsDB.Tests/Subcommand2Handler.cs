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
        await Task.Delay(1);
        await _taskMessaging.ProgressMessage(message, "started task Subcommand2");
        await Task.Delay(1);
        await _taskMessaging.CompleteTask(message);
    }
}