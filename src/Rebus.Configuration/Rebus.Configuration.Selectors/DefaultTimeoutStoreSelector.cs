using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Timeouts;

namespace Dbosoft.Rebus.Configuration;

public class DefaultTimeoutsStoreSelector: GenericRebusSelectorBase<ITimeoutManager>, IRebusTimeoutConfigurer
{

    public DefaultTimeoutsStoreSelector(IConfiguration configuration, ILogger log) : base(configuration, log)
    {
    }

    public override string[] AcceptedConfigTypes => new []{"inmemory" };
    public override string ConfigurationName => "bus";

    protected override void ConfigureByType(string busType, StandardConfigurer<ITimeoutManager> configurer)
    {
        switch (busType)
        {
            case "inmemory":
                configurer.StoreInMemory();
                break;
        }
    }



}