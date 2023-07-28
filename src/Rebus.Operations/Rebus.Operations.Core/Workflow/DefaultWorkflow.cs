#nullable enable
namespace Dbosoft.Rebus.Operations.Workflow;

public class DefaultWorkflow : IWorkflow
{
    public DefaultWorkflow(WorkflowOptions workflowOptions, IOperationManager operation, IOperationTaskManager tasks, IOperationMessaging messaging)
    {
        WorkflowOptions = workflowOptions;
        Operations = operation;
        Tasks = tasks;
        Messaging = messaging;
    }

    public WorkflowOptions WorkflowOptions { get; }
    public IOperationManager Operations { get; }
    public IOperationTaskManager Tasks { get; }
    public IOperationMessaging Messaging { get; }
}