namespace Dbosoft.Rebus
{
    public interface IRebusUnitOfWork : IAsyncDisposable, IDisposable
    {
        public Task Commit();
    }
}