using Rebus.Config;
using Rebus.DataBus;
using Rebus.Sagas;
using Rebus.Subscriptions;
using Rebus.Timeouts;

namespace Dbosoft.Rebus.Configuration;

public static class RebusConfigurerExtensions
{

    public static RebusConfigurer DataBus(this RebusConfigurer rebusConfigurer,
        IRebusConfigurer<IDataBusStorage> configurer)
    {
        rebusConfigurer.DataBus(configurer.Configure);
        return rebusConfigurer;
    }

    public static RebusConfigurer Timeouts(this RebusConfigurer rebusConfigurer,
        IRebusConfigurer<ITimeoutManager> configurer)
    {
        rebusConfigurer.Timeouts(configurer.Configure);
        return rebusConfigurer;
    }

    public static RebusConfigurer Subscriptions(this RebusConfigurer rebusConfigurer,
        IRebusConfigurer<ISubscriptionStorage> configurer)
    {
        rebusConfigurer.Subscriptions(configurer.Configure);
        return rebusConfigurer;
    }

    public static RebusConfigurer OneWayTransport(this RebusConfigurer rebusConfigurer,
        IRebusTransportConfigurer configurer)
    {
        rebusConfigurer.Transport(configurer.ConfigureAsOneWayClient);
        return rebusConfigurer;
    }

    public static RebusConfigurer Transport(this RebusConfigurer rebusConfigurer,
        IRebusTransportConfigurer configurer, string queueName)
    {
        rebusConfigurer.Transport(cfg => configurer.Configure(cfg, queueName));
        return rebusConfigurer;
    }

    public static RebusConfigurer Sagas(this RebusConfigurer rebusConfigurer,
        IRebusConfigurer<ISagaStorage> configurer)
    {
        rebusConfigurer.Sagas(configurer.Configure);
        return rebusConfigurer;
    }
}