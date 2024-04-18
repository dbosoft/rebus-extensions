using Rebus.Subscriptions;
using Rebus.Transport;
using Rebus.Transport.InMem;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class DefaultTransportSelectorTests : SelectorTests
{
    [Fact]
    public void Throws_on_Missing_bus_type()
    {
        var deps = SetupDeps(new Dictionary<string, string>());


        var selector =
            new DefaultTransportSelector(new InMemNetwork(), deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.OneWayTransport(selector),true));

    }

    [Fact]
    public void Throws_on_Invalid_bus_type()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "invalid" }
        });

        var selector =
            new DefaultTransportSelector(new InMemNetwork(), deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.OneWayTransport(selector)));

    }


    [Fact]
    public void Configures_As_InMemory()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "inmemory" }
        });


        var selector = new DefaultTransportSelector(new InMemNetwork(), deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c =>
            {
                c.OneWayTransport(selector);
            }, true)
            .AssertConfigured<ITransport>("Rebus.Transport.InMem.InMemTransport")
            .AssertConfigured<ISubscriptionStorage>("Rebus.Transport.InMem.InMemTransport");


        ConfigureAndGetContainer(c =>
            {
                c.Transport(selector, "dummy");
            }, true)
            .AssertConfigured<ITransport>("Rebus.Transport.InMem.InMemTransport")
            .AssertConfigured<ISubscriptionStorage>("Rebus.Transport.InMem.InMemTransport");
    }

    [Fact]
    public void Configures_As_InMemory_WithoutSubscriptionStore()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "inmemory" },
            { "bus:storeSubscriptions", "false" }
        });


        var selector = new DefaultTransportSelector(new InMemNetwork(), deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c =>
            {
                c.OneWayTransport(selector);
            }, true)
            .AssertConfigured<ITransport>("Rebus.Transport.InMem.InMemTransport")
            .AssertConfigured<ISubscriptionStorage>("Rebus.Persistence.Throwing.DisabledSubscriptionStorage");


        ConfigureAndGetContainer(c =>
            {
                c.Transport(selector, "dummy");
            }, true)
            .AssertConfigured<ITransport>("Rebus.Transport.InMem.InMemTransport")
            .AssertConfigured<ISubscriptionStorage>("Rebus.Persistence.Throwing.DisabledSubscriptionStorage");
    }

    [Fact]
    public void Configures_As_FileSystem_With_Path()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "filesystem" },
            { "bus:path", "%TEMP" }
        });


        var selector = new DefaultTransportSelector(new InMemNetwork(), deps.Configuration, deps.Logger);

        ConfigureAndGetContainer(c =>
            {
                c.OneWayTransport(selector);
            }, true)
            .AssertConfigured<ITransport>("Rebus.Transport.FileSystem.FileSystemTransport");


        ConfigureAndGetContainer(c =>
            {
                c.Transport(selector, "dummy");
            }, true)
            .AssertConfigured<ITransport>("Rebus.Transport.FileSystem.FileSystemTransport");

    }

    [Fact]
    public void Configures_As_FileSystem_Throws_if_Path_missing()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "filesystem" }
        });


        var selector = new DefaultTransportSelector(new InMemNetwork(), deps.Configuration, deps.Logger);

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.OneWayTransport(selector)));

        Assert.Throws<InvalidOperationException>(() => ConfigureAndGetContainer(c => c.Transport(selector, "dummy")));

    }


}