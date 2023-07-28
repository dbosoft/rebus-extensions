#nullable enable
namespace Dbosoft.Rebus.Operations.Workflow;

public interface IWorkflow
{
    IOperationManager Operations { get;  }
    IOperationTaskManager Tasks { get; }
    IOperationMessaging Messaging { get; }
    
    WorkflowOptions WorkflowOptions{ get; }
}