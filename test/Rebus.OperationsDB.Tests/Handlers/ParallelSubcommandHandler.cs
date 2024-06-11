using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.OperationsDB.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

public class ParallelSubcommandHandler(ITaskMessaging taskMessaging)
    : IHandleMessages<OperationTask<ParallelSubCommand>>
{
    public async Task Handle(OperationTask<ParallelSubCommand> message)
    {
        await taskMessaging.ProgressMessage(message, $"Started task {nameof(ParallelSubCommand)}")
            .ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);
        await taskMessaging.CompleteTask(message, new ParallelSubCommandResponse()
        {
            ItemId = message.Command.ItemId,
        }).ConfigureAwait(false);
    }
}
