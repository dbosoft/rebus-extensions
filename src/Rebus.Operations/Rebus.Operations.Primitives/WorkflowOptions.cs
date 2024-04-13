using System;
using System.Text.Json;

namespace Dbosoft.Rebus.Operations;

public class WorkflowOptions
{
    public WorkflowEventDispatchMode DispatchMode { get; set; } = WorkflowEventDispatchMode.Publish;
    public string? EventDestination { get; set; }
    
    public string? OperationsDestination { get; set; }
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new(JsonSerializerDefaults.Web);

    public TimeSpan DeferCompletion { get; set; } = TimeSpan.Zero;
}