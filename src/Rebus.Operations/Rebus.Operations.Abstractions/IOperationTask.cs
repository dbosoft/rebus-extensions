using System;

namespace Dbosoft.Rebus.Operations;

public interface IOperationTask
{
    public Guid Id { get; }

    public Guid OperationId { get; }

    public Guid InitiatingTaskId { get; }


    OperationTaskStatus Status { get; }

}