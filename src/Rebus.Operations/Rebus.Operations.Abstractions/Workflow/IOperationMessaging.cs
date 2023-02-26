using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;

namespace Dbosoft.Rebus.Operations.Workflow;

public interface IOperationMessaging
{
    void DispatchTaskMessage(object command, IOperationTask task);
    Task DispatchTaskStatusEventAsync(string commandType, OperationTaskStatusEvent message);
    Task DispatchTaskStatusEventAsync(OperationTaskStatusEvent message);
    Task DispatchOperationStatusEventAsync(OperationStatusEvent operationStatusEvent);

    IOperationDispatcher OperationDispatcher { get; }
    IOperationTaskDispatcher TaskDispatcher { get; }


}