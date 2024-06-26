﻿using System;

namespace Dbosoft.Rebus.Operations.Commands;

/// <summary>
/// Generic message that wraps a task message
/// </summary>
public class OperationTaskSystemMessage<TMessage> : IOperationTaskMessage
{

    // ReSharper disable once UnusedMember.Global
    public OperationTaskSystemMessage()
    {
    }

    public OperationTaskSystemMessage(
        TMessage message, Guid operationId, Guid initiatingTaskId, Guid taskId, DateTimeOffset created)
    {
        Message = message;
        OperationId = operationId;
        InitiatingTaskId = initiatingTaskId;
        TaskId = taskId;
        Created = created;
    }

    public TMessage? Message { get; set; }

    public Guid OperationId { get; set; }
    public Guid InitiatingTaskId { get; set; }


    public Guid TaskId { get; set; }
    public DateTimeOffset Created { get; set; }
}