using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Commands;
using Dbosoft.Rebus.Operations.Events;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Dbosoft.Rebus.Operations.Workflow;

public class OperationExclusiveAccessStep(int numberOfBuckets, CancellationToken cancellationToken)
    : IIncomingStep, IDisposable
{
    // This code is based on Rebus's SemaphoreSlimExclusiveSagaAccessIncomingStep

    private readonly SemaphoreSlim[] _locks = Enumerable.Range(0, numberOfBuckets)
        .Select(n => new SemaphoreSlim(1, 1))
        .ToArray();

    public Task Process(IncomingStepContext context, Func<Task> next)
    {
        var message = context.Load<Message>() ?? throw new ArgumentException(
            "The deserialized message is missing. The pipeline is most likely ordered incorrectly.",
            nameof(context));

        return message.Body switch
        {
            IOperationTaskMessage otm and (CreateNewOperationTaskCommand or OperationTaskAcceptedEvent or OperationTaskStatusEvent) =>
                RunWithLock(otm.OperationId, next),
            OperationTaskProgressEvent otpe =>
                RunWithLock(otpe.OperationId, next),
            _ => next()
        };
    }

    private async Task RunWithLock(Guid operationId, Func<Task> action)
    {
        await AcquireLockAsync(operationId);
        try
        {
            await action();
        }
        finally
        {
            ReleaseLock(operationId);
        }
    }

    private async Task AcquireLockAsync(Guid id)
    {
        var lockId = Math.Abs(id.GetHashCode() % numberOfBuckets);

        await _locks[lockId].WaitAsync(cancellationToken);
    }

    private void ReleaseLock(Guid id)
    {
        var lockId = Math.Abs(id.GetHashCode() % numberOfBuckets);

        _locks[lockId].Release();
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            foreach (var disposable in _locks)
            {
                disposable.Dispose();
            }
        }
        finally
        {
            _disposed = true;
        }
    }
}