using Rebus.Persistence.InMem;
using Rebus.Subscriptions;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class DefaultSubscriptionStoreSelectorTests : SelectorTests
{
    [Fact]
    public void Throws_on_Missing_bus_type()
    {
        var deps = SetupDeps(new Dictionary<string, string>());


        var selector =
            new DefaultSubscriptionStoreSelector(new InMemorySubscriberStore(), deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.Subscriptions(selector)));

    }

    [Fact]
    public void Throws_on_Invalid_bus_type()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "invalid" }
        });

        var selector =
            new DefaultSubscriptionStoreSelector(new InMemorySubscriberStore(), deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.Subscriptions(selector)));

    }


    [Fact]
    public void Configures_As_InMemory()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "inmemory" }
        });


        var selector = new DefaultSubscriptionStoreSelector(new InMemorySubscriberStore(), deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c => c.Subscriptions(selector))
            .AssertConfigured<ISubscriptionStorage>("Rebus.Persistence.InMem.InMemorySubscriptionStorage");

    }

    [Fact]
    public void Configures_As_FileSystem_With_FileName()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "filesystem" },
            { "store:subscriptionFile", "%TEMP%\\subfile" }
        });


        var selector = new DefaultSubscriptionStoreSelector(new InMemorySubscriberStore(), deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c => c.Subscriptions(selector))
            .AssertConfigured<ISubscriptionStorage>("Rebus.Persistence.FileSystem.JsonFileSubscriptionStorage");

    }

    [Fact]
    public void Configures_As_FileSystem_With_Path()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "filesystem" },
            { "store:path", "%TEMP" }
        });


        var selector = new DefaultSubscriptionStoreSelector(new InMemorySubscriberStore(), deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c => c.Subscriptions(selector))
            .AssertConfigured<ISubscriptionStorage>("Rebus.Persistence.FileSystem.JsonFileSubscriptionStorage");

    }

    [Fact]
    public void Configures_As_FileSystem_Throws_if_Path_missing()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "store:type", "filesystem" }
        });


        var selector = new DefaultSubscriptionStoreSelector(new InMemorySubscriberStore(), deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.Subscriptions(selector)));

    }


}