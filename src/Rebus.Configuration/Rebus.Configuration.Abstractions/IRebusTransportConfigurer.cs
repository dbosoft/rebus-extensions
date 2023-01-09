using Rebus.Config;
using Rebus.Transport;

namespace Dbosoft.Rebus.Configuration;

public interface IRebusTransportConfigurer
{
    void ConfigureAsOneWayClient(StandardConfigurer<ITransport> configurer);

    void Configure(StandardConfigurer<ITransport> configurer, string queueName);

}