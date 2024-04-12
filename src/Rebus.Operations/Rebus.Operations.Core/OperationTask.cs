using System;

namespace Dbosoft.Rebus.Operations
{
    public class OperationTask<T> : IOperationTaskMessage where T : class, new()
    {

        public OperationTask(T command, Guid operationId, Guid initiatingTaskId, Guid taskId, 
            DateTimeOffset created)
        {
            Command = command;
            OperationId = operationId;
            InitiatingTaskId = initiatingTaskId;
            TaskId = taskId;
            Created = created;
        }

        public T Command { get; }
        public Guid OperationId { get; }
        public Guid InitiatingTaskId { get; }
        public Guid TaskId { get; }
        public DateTimeOffset Created { get; }
    }
}