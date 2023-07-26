using Rebus.Handlers;
using Rebus.Pipeline;

namespace Dbosoft.Rebus.Operations.Tests;

public class ExposingHeadersCommandHandler : IHandleMessages<OperationTask<TestCommand>>
{
    private readonly ITaskMessaging _messaging;
    
    public ExposingHeadersCommandHandler(ITaskMessaging messaging)
    {
        _messaging = messaging;
    }

    public static bool Called { get; set; }
    public static IDictionary<string, string>? Headers;

    public Task Handle(OperationTask<TestCommand> message)
    {
        Called = true;
        Headers = MessageContext.Current.Headers;
        return _messaging.CompleteTask(message);
    }
}