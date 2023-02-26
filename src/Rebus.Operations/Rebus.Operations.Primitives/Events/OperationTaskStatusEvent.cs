using System;

namespace Dbosoft.Rebus.Operations.Events
{
    public class OperationTaskStatusEvent : OperationTaskStatusEventBase
    {
        // ReSharper disable once UnusedMember.Global
        // required for serialization
        public OperationTaskStatusEvent()
        {
        }

        protected OperationTaskStatusEvent(Guid operationId, Guid initiatingTaskId, Guid taskId, bool failed,
            string? messageType,
            string? messageData) : base(operationId, initiatingTaskId, taskId, failed, messageType, messageData)
        {

        }
        
        public static OperationTaskStatusEvent Failed(Guid operationId, Guid initiatingTaskId, Guid taskId)
        {
            return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, true, null, null);
        }

        public static OperationTaskStatusEvent Failed(Guid operationId, Guid initiatingTaskId, Guid taskId, object? message)
        {
            var (data, typeName) = SerializeMessage(message);
            return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, true, typeName, data);
        }

        public static OperationTaskStatusEvent Completed(Guid operationId, Guid initiatingTaskId, Guid taskId)
        {
            return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, false, null, null);
        }

        public static OperationTaskStatusEvent Completed(Guid operationId, Guid initiatingTaskId, Guid taskId, object? message)
        {
            var (data, typeName) = SerializeMessage(message);
            return new OperationTaskStatusEvent(operationId, initiatingTaskId,taskId, false, typeName, data);
        }

    }


    // ReSharper disable once UnusedTypeParameter
        public class OperationTaskStatusEvent<T> : OperationTaskStatusEventBase where T : class, new()
    {

        public OperationTaskStatusEvent()
        {
        }

        public OperationTaskStatusEvent(OperationTaskStatusEvent message) :
            base(message.OperationId, message.InitiatingTaskId, message.TaskId, message.OperationFailed, message.MessageType, message.MessageData)
        {
        }
    }
}