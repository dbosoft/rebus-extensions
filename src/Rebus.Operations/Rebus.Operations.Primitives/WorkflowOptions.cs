namespace Dbosoft.Rebus.Operations;

public class WorkflowOptions
{
    public WorkflowEventDispatchMode DispatchMode { get; set; } = WorkflowEventDispatchMode.Publish;
    public string? EventDestination { get; set; }
    
    public string? OperationsDestination { get; set; }
}