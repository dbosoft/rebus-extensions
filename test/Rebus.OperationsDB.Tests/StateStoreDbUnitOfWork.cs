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

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public Task Commit()
    {
        _logger.LogInformation("COMMIT of State Store");
        return _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
    }
}