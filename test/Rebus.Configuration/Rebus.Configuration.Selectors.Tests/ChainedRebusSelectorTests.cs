using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.DataBus;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class ChainedRebusSelectorTests : SelectorTests
{
    [Fact]
    public void Configures_Calls_Chain()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "databus:type", "bus2" },
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);
        var selector2 = new DummySelector(new[] { "bus2" }, deps.Configuration, deps.Logger, false);

        var chain = new ChainedRebusSelector<IDataBusStorage>(deps.Configuration, deps.Logger,
            new[] { selector1, selector2 });


        CreateConfigurer(c => c.DataBus(chain), false);

        Assert.Null(selector1.ConfigureBusTypeWithBusType);
        Assert.Equal("bus2", selector2.ConfigureBusTypeWithBusType);

    }

    [Fact]
    public void Empty_Selector_throws_ArgumentException()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "databus:type", "bus2" },
        });

        Assert.Throws<ArgumentException>(() =>
            new ChainedRebusSelector<IDataBusStorage>(deps.Configuration, deps.Logger, 
                Array.Empty<GenericRebusSelectorBase<IDataBusStorage>>())
        );


    }

    [Fact]
    public void Configures_Calls_First_Selector()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "databus:type", "bus1" },
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);
        var selector2 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);

        var chain = new ChainedRebusSelector<IDataBusStorage>(deps.Configuration, deps.Logger, new[] { selector1, selector2 });


        CreateConfigurer(c => c.DataBus(chain), true);

        Assert.Null(selector2.ConfigureBusTypeWithBusType);
        Assert.Equal("bus1", selector1.ConfigureBusTypeWithBusType);

    }

    [Fact]
    public void Configures_Calls_Next_selector_if_failed()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "databus:type", "bus1" },
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, true);
        var selector2 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);

        var chain = new ChainedRebusSelector<IDataBusStorage>(deps.Configuration, deps.Logger, new[] { selector1, selector2 });

        CreateConfigurer(c => c.DataBus(chain), true);

        Assert.Null(selector1.ConfigureBusTypeWithBusType);
        Assert.Equal("bus1", selector2.ConfigureBusTypeWithBusType);

    }

    [Fact]
    public void Empty_Selectors_throws_ArgumentException()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "bus2" },
        });


        Assert.Throws<ArgumentException>(() =>
            new ChainedRebusSelector<IDataBusStorage>(deps.Configuration, deps.Logger, Array.Empty<GenericRebusSelectorBase<IDataBusStorage>>())
        );


    }

    [Fact]
    public void Mixed_Selector_throws_ArgumentException()
    {
        var deps = SetupDeps(new Dictionary<string, string>
        {
            { "bus:type", "bus2" },
        });

        var selector1 = new DummySelector(new[] { "bus1" }, deps.Configuration, deps.Logger, false);
        var selector2 = new DummySelector(new[] { "bus2" }, deps.Configuration, deps.Logger, false, "pengbus");


        Assert.Throws<ArgumentException>(() =>
            new ChainedRebusSelector<IDataBusStorage>(deps.Configuration, deps.Logger, new[] { selector1, selector2 })
        );


    }


    public class DummySelector : GenericRebusSelectorBase<IDataBusStorage>, IRebusDataBusConfigurer
    {
        private readonly bool _fails;

        public DummySelector(string[] acceptedConfigTypes, IConfiguration configuration, ILogger log, bool fails, 
            string configName = "databus") 
            
            : base(configuration, log)
        {
            _fails = fails;
            AcceptedConfigTypes = acceptedConfigTypes;
            ConfigurationName = configName;
        }

        public string ConfigureBusTypeWithBusType;


        public override string[] AcceptedConfigTypes { get; }
        public override string ConfigurationName { get;  }
        protected override void ConfigureByType(string type, StandardConfigurer<IDataBusStorage> configurer)
        {
            if(_fails)
                throw new NotImplementedException();

            ConfigureBusTypeWithBusType = type;
        }
    }
}