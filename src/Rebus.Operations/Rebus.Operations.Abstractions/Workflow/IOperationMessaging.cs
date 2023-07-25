using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;

namespace Dbosoft.Rebus.Operations.Workflow;

public interface IOperationMessaging
{
    Task DispatchTaskMessage(object command, IOperationTask task, IDictionary<string,string>? additionalHeaders = null);
    Task DispatchTaskStatusEventAsync(string commandType, OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null);
    Task DispatchTaskStatusEventAsync(OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null);
    Task DispatchOperationStatusEventAsync(OperationStatusEvent operationStatusEvent, IDictionary<string,string>? additionalHeaders = null);

    IOperationDispatcher OperationDispatcher { get; }
    IOperationTaskDispatcher TaskDispatcher { get; }

    Task FailTask(IOperationTaskMessage message, string errorMessage,
        IDictionary<string, string>? additionalHeaders = null);

    Task FailTask(IOperationTaskMessage message, ErrorData error,
        IDictionary<string, string>? additionalHeaders = null);


    Task CompleteTask(IOperationTaskMessage message, IDictionary<string, string>? additionalHeaders = null);

    Task CompleteTask(IOperationTaskMessage message, object responseMessage,
        IDictionary<string, string>? additionalHeaders = null);


    Task ProgressMessage(IOperationTaskMessage message, object data,
        IDictionary<string, string>? additionalHeaders = null);
}