using JetBrains.Annotations;
using System;
using System.Text.Json;

namespace Dbosoft.Rebus.Operations.Events;

[PublicAPI]
public class OperationTaskStatusEvent : OperationTaskStatusEventBase
{
    // ReSharper disable once UnusedMember.Global
    // required for serialization
    public OperationTaskStatusEvent()
    {
    }

    protected OperationTaskStatusEvent(Guid operationId, Guid initiatingTaskId, Guid taskId, bool failed,
        bool cancelled,
        string? messageType,
        string? messageData) : base(operationId, initiatingTaskId, taskId, DateTimeOffset.UtcNow, failed, cancelled, messageType, messageData)
    {

    }

    public static OperationTaskStatusEvent Failed(Guid operationId, Guid initiatingTaskId, Guid taskId)
    {
        return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, true, false, null, null);
    }

    public static OperationTaskStatusEvent Failed(Guid operationId, Guid initiatingTaskId, Guid taskId, object? message, JsonSerializerOptions serializerOptions)
    {
        var (data, typeName) = SerializeMessage(message, serializerOptions);
        return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, true, false, typeName, data);
    }

    public static OperationTaskStatusEvent Completed(Guid operationId, Guid initiatingTaskId, Guid taskId)
    {
        return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, false, false, null, null);
    }

    public static OperationTaskStatusEvent Completed(Guid operationId, Guid initiatingTaskId, Guid taskId, object? message, JsonSerializerOptions serializerOptions)
    {
        var (data, typeName) = SerializeMessage(message, serializerOptions);
        return new OperationTaskStatusEvent(operationId, initiatingTaskId,taskId, false, false, typeName, data);
    }

    public static OperationTaskStatusEvent Cancelled(Guid operationId, Guid initiatingTaskId, Guid taskId)
    {
        return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, false, true, null, null);
    }

    public static OperationTaskStatusEvent Cancelled(Guid operationId, Guid initiatingTaskId, Guid taskId, object? message, JsonSerializerOptions serializerOptions)
    {
        var (data, typeName) = SerializeMessage(message, serializerOptions);
        return new OperationTaskStatusEvent(operationId, initiatingTaskId, taskId, false, true, typeName, data);
    }

}


// ReSharper disable once UnusedTypeParameter
public class OperationTaskStatusEvent<T> : OperationTaskStatusEventBase where T : class, new()
{

    public OperationTaskStatusEvent()
    {
    }

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public OperationTaskStatusEvent(OperationTaskStatusEvent message) :
        base(message.OperationId, message.InitiatingTaskId, message.TaskId, message.Created, message.OperationFailed, message.OperationCancelled, message.MessageType, message.MessageData)
    {
    }
}