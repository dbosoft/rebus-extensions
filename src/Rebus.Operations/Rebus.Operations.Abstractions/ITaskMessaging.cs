using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations;

public interface ITaskMessaging
{
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