using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;

namespace Dbosoft.Rebus.Configuration;

public abstract class GenericRebusSelectorBase<TConfigurer> : RebusSelectorBase, IRebusConfigurer<TConfigurer>
{
    private readonly ILogger _log;

    protected GenericRebusSelectorBase(IConfiguration configuration, ILogger log) : base(configuration)
    {
        _log = log;
    }

    public virtual void Configure(StandardConfigurer<TConfigurer> configurer)
    {
        var busType = Configuration[$"{ConfigurationName}:type"];
        var busTypeNames = string.Join(",", AcceptedConfigTypes);


        if (busType == null)
            throw new InvalidOperationException(
                $"Missing configuration entry for {ConfigurationName}::type. Configure a valid {ConfigurationName} type ({busTypeNames})");

        if (!AcceptedConfigTypes.Contains(busType))
            throw new InvalidOperationException(
                $"Invalid {ConfigurationName} type: '{busType}'. Configure a valid {ConfigurationName} type ({busTypeNames})");

        _log.LogInformation("Configuring Rebus {configurationName} type as '{busType}'", ConfigurationName, busType );
        ConfigureByType(busType, configurer);

    }

    protected abstract void ConfigureByType(string type, StandardConfigurer<TConfigurer> configurer);


}