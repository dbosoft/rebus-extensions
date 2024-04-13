using JetBrains.Annotations;
using System;
using System.Text.Json;

namespace Dbosoft.Rebus.Operations.Events;

[PublicAPI]
public class OperationTaskStatusEventBase : IOperationTaskStatusEvent
{
    public OperationTaskStatusEventBase() {}

    protected OperationTaskStatusEventBase(Guid operationId, Guid initiatingTaskId, Guid taskId,
        DateTimeOffset created,
        bool failed, string? messageType,
        string? messageData)
    {
        OperationId = operationId;
        InitiatingTaskId = initiatingTaskId;
        TaskId = taskId;
        OperationFailed = failed;
        MessageData = messageData;
        MessageType = messageType;
        Created = created;
    }

    public string? MessageData { get; set; }
    public string? MessageType { get; set; }
    public bool OperationFailed { get; set; }
    public Guid OperationId { get; set; }
    public Guid TaskId { get; set; }
    public DateTimeOffset Created { get; set;  }
    public Guid InitiatingTaskId { get; set; }

    protected static (string? data, string? type) SerializeMessage(object? message, JsonSerializerOptions serializerOptions)
    {
        return message == null 
            ? (null, null) 
            : (JsonSerializer.Serialize(message, serializerOptions), message.GetType().AssemblyQualifiedName);
    }

    public object? GetMessage(JsonSerializerOptions serializerOptions)
    {
        if (MessageData == null || MessageType == null)
            return null;

        var type = Type.GetType(MessageType);

        return type == null
            ? null
            : JsonSerializer.Deserialize(MessageData, type, serializerOptions);
    }

    public T? GetErrorDetails<T>(JsonSerializerOptions serializerOptions)
    {
        if (MessageData == null || MessageType == null)
            return default;

        var type = Type.GetType(MessageType);

        if (type == null) return default;

        if (type != typeof(ErrorData) && !type.IsSubclassOf(typeof(ErrorData))) return default;

        var data = JsonSerializer.Deserialize(MessageData, type, serializerOptions) as ErrorData;

        if ( data?.AdditionalData is JsonElement element)
        {
            return element.Deserialize<T>(serializerOptions);
        }

        return default;

    }
}