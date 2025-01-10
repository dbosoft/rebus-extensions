namespace Dbosoft.Rebus;

public interface IRebusUnitOfWork : IAsyncDisposable, IDisposable
{
    public Task Initialize();

    public Task Commit();

    public Task Rollback();
}