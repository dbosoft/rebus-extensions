using Rebus.Sagas;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class DefaultSagaStoreSelectorTests : SelectorTests
{
    [Fact]
    public void Throws_on_Missing_bus_type()
    {
        var deps = SetupDeps(new Dictionary<string, string>());


        var selector =
            new DefaultSagaStoreSelector(deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.Sagas(selector)));

    }

    [Fact]
    public void Throws_on_Invalid_bus_type()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "invalid" }
        });

        var selector =
            new DefaultSagaStoreSelector(deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.Sagas(selector)));

    }


    [Fact]
    public void Configures_As_InMemory()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "inmemory" }
        });


        var selector = new DefaultSagaStoreSelector(deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c => c.Sagas(selector))
            .AssertConfigured<ISagaStorage>("Rebus.Persistence.InMem.InMemorySagaStorage");

    }
    
    [Fact]
    public void Configures_As_FileSystem_With_Path()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "filesystem" },
            { "store:path", "%TEMP" }
        });


        var selector = new DefaultSagaStoreSelector(deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c => c.Sagas(selector))
            .AssertConfigured<ISagaStorage>("Rebus.Persistence.FileSystem.FileSystemSagaStorage");

    }

    [Fact]
    public void Configures_As_FileSystem_Throws_if_Path_missing()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "filesystem" }
        });


        var selector = new DefaultSagaStoreSelector(deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.Sagas(selector)));

    }


}