using Dbosoft.Rebus.Operations.Workflow;

namespace Dbosoft.Rebus.Operations.Tests.Data;

public static class OperationManagerExtensions
{
    public static async Task WaitForOperation(
        this IOperationManager operationManager,
        Guid operationId)
    {
        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTimeOffset.UtcNow;

        IOperation? operation;
        do
        {
            await Task.Delay(100);
            operation = await operationManager.GetByIdAsync(operationId);
        } while (operation is not { Status: OperationStatus.Failed or OperationStatus.Completed }
                 && DateTimeOffset.UtcNow - start <= timeout);
    }
}
