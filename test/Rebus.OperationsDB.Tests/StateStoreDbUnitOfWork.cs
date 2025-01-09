using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public sealed class StateStoreDbUnitOfWork : IRebusUnitOfWork
{
    private readonly StateStoreContext _dbContext;
    private readonly ILogger _logger;

    public StateStoreDbUnitOfWork(StateStoreContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task Initialize() => Task.CompletedTask;

    public async Task Commit()
    {
        _logger.LogInformation("COMMIT of State Store");
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task Rollback() => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
    }
}