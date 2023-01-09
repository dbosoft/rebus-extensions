using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Transport;

namespace Dbosoft.Rebus.Configuration;

public abstract class RebusTransportSelectorBase : RebusSelectorBase, IRebusTransportConfigurer
{
    private readonly ILogger _log;
    public override string ConfigurationName => "bus";

    protected RebusTransportSelectorBase(IConfiguration configuration, ILogger log) : base(configuration)
    {
        _log = log;
    }


    protected abstract void ConfigureBusTypeAsOneWayClient(string busType, StandardConfigurer<ITransport> configurer);

    protected abstract void ConfigureBusType(string busType, string queueName,
        StandardConfigurer<ITransport> configurer);

    public void ConfigureAsOneWayClient(StandardConfigurer<ITransport> configurer)
    {

        var busType = GetBusType();
        _log.LogInformation("Configuring Rebus {configurationName} type as '{busType}'", ConfigurationName, busType);
        ConfigureBusTypeAsOneWayClient(busType, configurer);
    }

    public void Configure(StandardConfigurer<ITransport> configurer, string queueName)
    {
        var busType = GetBusType();
        _log.LogInformation("Configuring Rebus {configurationName} type as '{busType}'", ConfigurationName, busType);
        ConfigureBusType(busType, queueName, configurer);

    }

    private string GetBusType()
    {
        var busType = Configuration[$"{ConfigurationName}:type"];
        var busTypeNames = string.Join(",", AcceptedConfigTypes);


        if (busType == null)
            throw new InvalidOperationException(
                $"Missing configuration entry for {ConfigurationName}::type. Configure a valid {ConfigurationName} type ({busTypeNames})");

        if (!AcceptedConfigTypes.Contains(busType))
            throw new InvalidOperationException(
                $"Invalid {ConfigurationName} type: '{busType}'. Configure a valid {ConfigurationName} type ({busTypeNames})");

        return busType;
    }

}