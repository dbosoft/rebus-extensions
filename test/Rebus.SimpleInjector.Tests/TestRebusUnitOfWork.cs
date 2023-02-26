namespace Dbosoft.Rebus.SimpleInjector.Tests;

public class TestRebusUnitOfWork : IRebusUnitOfWork
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        
    }

    public Task Commit()
    {
        return Task.CompletedTask;
    }
}