using Dbosoft.Rebus.Operations.Tests.Commands;
using Rebus.Handlers;
using Rebus.Pipeline;

namespace Dbosoft.Rebus.Operations.Tests.Handlers;

public class UseHeadersCommandHandler(
    ITaskMessaging messaging,
    TestTrace trace)
    : IHandleMessages<OperationTask<UseHeadersCommand>>
{
    public async Task Handle(OperationTask<UseHeadersCommand> message)
    {
        MessageContext.Current.Headers.TryGetValue("custom_header", out var headerValue);
        trace.Trace(this, nameof(Handle), message, headerValue);
        await messaging.CompleteTask(message);
    }
}
