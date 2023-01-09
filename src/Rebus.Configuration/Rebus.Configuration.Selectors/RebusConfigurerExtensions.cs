using Rebus.Config;

namespace Dbosoft.Rebus.Configuration
{
    public static class RebusConfigurerExtensions
    {

        public static RebusConfigurer DataBus(this RebusConfigurer rebusConfigurer,
            IRebusDataBusConfigurer configurer)
        {
            rebusConfigurer.DataBus(configurer.Configure);
            return rebusConfigurer;
        }


        public static RebusConfigurer Timeouts(this RebusConfigurer rebusConfigurer,
            IRebusTimeoutConfigurer configurer)
        {
            rebusConfigurer.Timeouts(configurer.Configure);
            return rebusConfigurer;
        }

        public static RebusConfigurer Subscriptions(this RebusConfigurer rebusConfigurer,
            IRebusSubscriptionConfigurer configurer)
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
    }
}
