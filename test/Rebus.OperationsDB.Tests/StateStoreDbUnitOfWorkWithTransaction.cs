using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public sealed class StateStoreDbUnitOfWorkWithTransaction : IRebusUnitOfWork
{
    private readonly StateStoreContext _dbContext;
    private IDbContextTransaction? _dbTransaction;
    private readonly ILogger _logger;

    public StateStoreDbUnitOfWorkWithTransaction(StateStoreContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Initialize()
    {
        _dbTransaction = await _dbContext.Database.BeginTransactionAsync()
            .ConfigureAwait(false);
    }

    public async Task Commit()
    {
        if (_dbTransaction is null)
            throw new InvalidOperationException("The unit of work has not been initialized.");

        _logger.LogInformation("COMMIT of State Store");
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        await _dbTransaction.CommitAsync().ConfigureAwait(false);
    }

    public async Task Rollback()
    {
        if (_dbTransaction is null)
            throw new InvalidOperationException("The unit of work has not been initialized.");

        await _dbTransaction.RollbackAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbTransaction is not null)
            await _dbTransaction.DisposeAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _dbTransaction?.Dispose();
    }
}