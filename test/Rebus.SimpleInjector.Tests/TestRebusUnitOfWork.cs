namespace Dbosoft.Rebus.SimpleInjector.Tests;

public sealed class TestRebusUnitOfWork : IRebusUnitOfWork
{
    public Task Initialize()
    {
        return Task.CompletedTask;
    }

    public Task Commit()
    {
        return Task.CompletedTask;
    }

    public Task Rollback()
    {
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {

    }
}
