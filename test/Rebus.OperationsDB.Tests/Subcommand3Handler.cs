using Dbosoft.Rebus.Operations;
using Rebus.Handlers;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class Subcommand3Handler : IHandleMessages<OperationTask<SubCommand3>>
{
    private readonly ITaskMessaging _taskMessaging;

    public Subcommand3Handler(ITaskMessaging taskMessaging)
    {
        _taskMessaging = taskMessaging;
    }
    
    public async Task Handle(OperationTask<SubCommand3> message)
    {
        await _taskMessaging.ProgressMessage(message, "started task Subcommand3");
        await Task.Delay(1);
        await _taskMessaging.ProgressMessage(message, 50);
        await Task.Delay(1);
        await _taskMessaging.ProgressMessage(message, 80);
        await Task.Delay(500);
        await _taskMessaging.CompleteTask(message);
    }
}
