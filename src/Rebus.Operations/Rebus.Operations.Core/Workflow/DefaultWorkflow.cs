#nullable enable
namespace Dbosoft.Rebus.Operations.Workflow;

public class DefaultWorkflow : IWorkflow
{
    public DefaultWorkflow(IOperationManager operation, IOperationTaskManager tasks, IOperationMessaging messaging)
    {
        Operations = operation;
        Tasks = tasks;
        Messaging = messaging;
    }

    public IOperationManager Operations { get; }
    public IOperationTaskManager Tasks { get; }
    public IOperationMessaging Messaging { get; }
}