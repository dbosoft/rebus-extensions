using Ardalis.Specification.EntityFrameworkCore;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public class StateStoreRepository<T> : RepositoryBase<T>, IStateStoreRepository<T> where T : class
{

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public StateStoreRepository(StateStoreContext dbContext) : base(dbContext)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
        // return base.SaveChangesAsync(cancellationToken);
    }
}