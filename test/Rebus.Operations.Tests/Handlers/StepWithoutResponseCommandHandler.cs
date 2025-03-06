using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class StepWithoutResponseCommandHandler(
    ITaskMessaging taskMessaging,
    TestTracer tracer)
    : IHandleMessages<OperationTask<StepWithoutResponseCommand>>
{
    public async Task Handle(OperationTask<StepWithoutResponseCommand> message)
    {
        tracer.Trace(this, nameof(Handle), message);
        await taskMessaging.CompleteTask(message);
    }
}
