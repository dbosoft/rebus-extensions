using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using JetBrains.Annotations;

namespace Dbosoft.Rebus.Operations.Workflow;

[PublicAPI]
public interface IOperationMessaging
{
    Task DispatchTaskMessage(object command, IOperationTask task, IDictionary<string,string>? additionalHeaders = null);
    Task DispatchTaskStatusEventAsync(string commandType, OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null);
    Task DispatchTaskStatusEventAsync(OperationTaskStatusEvent message, IDictionary<string,string>? additionalHeaders = null);
    Task DispatchOperationStatusEventAsync(OperationStatusEvent operationStatusEvent, IDictionary<string,string>? additionalHeaders = null);

    IOperationDispatcher OperationDispatcher { get; }
    IOperationTaskDispatcher TaskDispatcher { get; }

    Task SendDeferredMessage(object message, TimeSpan defer);
    Task DeferredCurrentMessage(TimeSpan defer);

}