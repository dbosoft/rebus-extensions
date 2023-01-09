using Rebus.Timeouts;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class DefaultTimeoutStoreSelectorTests : SelectorTests
{
    [Fact]
    public void Configures_As_InMemory()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "inmemory" }
        });


        var selector = new DefaultTimeoutsStoreSelector(deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c => c.Timeouts(selector))
            .AssertConfigured<ITimeoutManager>("Rebus.Persistence.InMem.InMemoryTimeoutManager");

    }
    
}