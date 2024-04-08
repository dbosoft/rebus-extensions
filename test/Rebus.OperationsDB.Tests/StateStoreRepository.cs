using Ardalis.Specification.EntityFrameworkCore;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class StateStoreRepository<T> : RepositoryBase<T>, IStateStoreRepository<T> where T : class
{

    public StateStoreRepository(StateStoreContext dbContext) : base(dbContext)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(0);
        // return base.SaveChangesAsync(cancellationToken);
    }
}