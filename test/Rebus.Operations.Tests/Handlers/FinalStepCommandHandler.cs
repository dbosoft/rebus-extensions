using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class FinalStepCommandHandler(
    ITaskMessaging messaging,
    TestTracer tracer)
    : IHandleMessages<OperationTask<FinalStepCommand>>
{
    public async Task Handle(OperationTask<FinalStepCommand> message)
    {
        tracer.Trace(this, nameof(Handle), message);
        await messaging.CompleteTask(message);
    }
}
