using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Transport;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class ChainedTransportSelectorTests : SelectorTests
{
    [Fact]
    public void Configures_Calls_Chain()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "bus2" }
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);
        var selector2 = new DummySelector(new[] { "bus2" }, deps.Configuration, deps.Logger, false);

        var chain = new ChainedTransportSelector(deps.Configuration, deps.Logger, new[] { selector1, selector2 });

        CreateConfigurer(c => c.OneWayTransport(chain), true);
        
        Assert.Null(selector1.ConfigureBusTypeAsOneWayClientWithBusType);
        Assert.Equal("bus2", selector2.ConfigureBusTypeAsOneWayClientWithBusType);


        CreateConfigurer(c => c.Transport(chain, "dummy"), true);

        Assert.Null(selector1.ConfigureBusTypeWithBusType);
        Assert.Equal("bus2", selector2.ConfigureBusTypeWithBusType);

    }

    [Fact]
    public void Mixed_Selector_throws_ArgumentException()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "bus2" }
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);
        var selector2 = new DummySelector(new[] { "bus2" }, deps.Configuration, deps.Logger, false, "pengbus");


        Assert.Throws<ArgumentException>(() =>
            new ChainedTransportSelector(deps.Configuration, deps.Logger, new []{selector1, selector2})
        );

        
    }

    [Fact]
    public void Empty_Selectors_throws_ArgumentException()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "bus2" }
        });

        Assert.Throws<ArgumentException>(() =>
            new ChainedTransportSelector(deps.Configuration, deps.Logger, Array.Empty<RebusTransportSelectorBase>())
        );


    }

    [Fact]
    public void Configures_Calls_First_Selector()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "bus1" }
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);
        var selector2 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);

        var chain = new ChainedTransportSelector(deps.Configuration, deps.Logger, new[] { selector1, selector2 });

        CreateConfigurer(c => c.OneWayTransport(chain), true);

        Assert.Null(selector2.ConfigureBusTypeAsOneWayClientWithBusType);
        Assert.Equal("bus1", selector1.ConfigureBusTypeAsOneWayClientWithBusType);


        CreateConfigurer(c => c.Transport(chain, "dummy"), true);

        Assert.Null(selector2.ConfigureBusTypeWithBusType);
        Assert.Equal("bus1", selector1.ConfigureBusTypeWithBusType);

    }

    [Fact]
    public void Configures_Calls_Next_selector_if_failed()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "bus1" }
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, true);
        var selector2 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);

        var chain = new ChainedTransportSelector(deps.Configuration, deps.Logger, new[] { selector1, selector2 });

        CreateConfigurer(c => c.OneWayTransport(chain), true);

        Assert.Null(selector1.ConfigureBusTypeAsOneWayClientWithBusType);
        Assert.Equal("bus1", selector2.ConfigureBusTypeAsOneWayClientWithBusType);


        CreateConfigurer(c => c.Transport(chain, "dummy"), true);

        Assert.Null(selector1.ConfigureBusTypeWithBusType);
        Assert.Equal("bus1", selector2.ConfigureBusTypeWithBusType);

    }


    public class DummySelector : RebusTransportSelectorBase
    {
        private readonly bool _fails;

        public DummySelector(string[] acceptedConfigTypes, IConfiguration configuration, ILogger log, bool fails, string configName = "bus") 
            
            : base(configuration, log)
        {
            _fails = fails;
            AcceptedConfigTypes = acceptedConfigTypes;
            ConfigurationName = configName;
        }

        public string? ConfigureBusTypeAsOneWayClientWithBusType;
        public string? ConfigureBusTypeWithBusType;
        public override string ConfigurationName { get; }

        public override string[] AcceptedConfigTypes { get; }
        protected override void ConfigureBusTypeAsOneWayClient(string busType, StandardConfigurer<ITransport> configurer)
        {

            if (_fails)
                throw new IOException();

            ConfigureBusTypeAsOneWayClientWithBusType = busType;

        }

        protected override void ConfigureBusType(string busType, string queueName, StandardConfigurer<ITransport> configurer)
        {

            if (_fails)
                throw new IOException();

            ConfigureBusTypeWithBusType = busType;

        }
    }
}