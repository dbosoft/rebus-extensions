using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Transport;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class MsmqTransportSelector : RebusTransportSelectorBase
{

    public MsmqTransportSelector(IConfiguration configuration, ILogger log) : base(configuration, log)
    {
    }

    public override string[] AcceptedConfigTypes => new[] { "msmq" };

    protected override void ConfigureBusType(string busType, string queueName, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "msmq":
                configurer.UseMsmq(queueName);
                return;
        }
    }

    protected override void ConfigureBusTypeAsOneWayClient(string busType, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "msmq":
                configurer.UseMsmqAsOneWayClient();
                return;
        }
    }



}