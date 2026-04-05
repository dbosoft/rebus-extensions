using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public abstract class PostgreSqlSelectorBase<TConfigurer> : GenericRebusSelectorBase<TConfigurer>
{

    protected PostgreSqlSelectorBase(IConfiguration configuration,
        ILogger log) : base(configuration, log)
    {
    }

    public override string[] AcceptedConfigTypes => new[] { "postgresql" };

    protected override void ConfigureByType(string busType, StandardConfigurer<TConfigurer> configurer)
    {
        switch (busType)
        {
            case "postgresql":

                var connectionString = Configuration[$"{ConfigurationName}:connectionstring"];

                if (connectionString == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::connectionstring.");

                ConfigurePostgreSql(configurer, connectionString);

                break;
        }
    }

    protected abstract void ConfigurePostgreSql(StandardConfigurer<TConfigurer> configurer, string connectionString);
}
