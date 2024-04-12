using System;

namespace Dbosoft.Rebus.Operations
{
    /// <summary>
    ///     Interface for all Messages for operation tasks
    /// </summary>
    public interface IOperationTaskMessage
    {
        Guid OperationId { get; }

        Guid InitiatingTaskId { get; }

        Guid TaskId { get;  }
        DateTimeOffset Created { get; }
    }
}