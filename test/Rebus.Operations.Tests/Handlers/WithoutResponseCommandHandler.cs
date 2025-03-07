using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class WithoutResponseCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<WithoutResponseCommand>>
{
    public async Task Handle(OperationTask<WithoutResponseCommand> message)
    {
        trace.Trace(this, nameof(Handle), message);
        await messaging.ProgressMessage(message, $"{nameof(WithoutResponseCommandHandler)}-1");
        await messaging.ProgressMessage(message, $"{nameof(WithoutResponseCommandHandler)}-2");
        await messaging.CompleteTask(message);
    }
}
