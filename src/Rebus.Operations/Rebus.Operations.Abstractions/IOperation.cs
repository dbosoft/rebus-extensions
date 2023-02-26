using System;

namespace Dbosoft.Rebus.Operations;

public interface IOperation
{
    public Guid Id { get; }

    public OperationStatus Status { get; }


}