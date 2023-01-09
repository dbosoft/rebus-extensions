using Rebus.DataBus;
using Rebus.DataBus.InMem;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class DefaultRebusDataBusSelectorTests : SelectorTests
{
    [Fact]
    public void Configures_As_InMemory()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "databus:type", "inmemory" }
        });
        
        var selector =
            new DefaultRebusDataBusSelector(deps.Configuration, new InMemDataStore(), deps.Logger);

        ConfigureAndGetContainer(c => c.DataBus(selector))
            .AssertConfigured<IDataBusStorage>("Rebus.DataBus.InMem.InMemDataBusStorage");

    }

    [Fact]
    public void Configures_As_FileSystem()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "databus:type", "filesystem" },
            { "databus:path", "%TEMP%" }

        });


        var selector = new DefaultRebusDataBusSelector(deps.Configuration, new InMemDataStore(), deps.Logger);

        ConfigureAndGetContainer(c => c.DataBus(selector))
            .AssertConfigured<IDataBusStorage>("Rebus.DataBus.FileSystem.FileSystemDataBusStorage");

    }

    [Fact]
    public void Configures_As_FileSystem_Throws_If_Path_Missing()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "databus:type", "filesystem" }
        });

        var selector = new DefaultRebusDataBusSelector(deps.Configuration, new InMemDataStore(), deps.Logger);

        Assert.Throws<InvalidOperationException>(() =>
            ConfigureAndGetContainer(c => c.DataBus(selector))
            );


    }

}