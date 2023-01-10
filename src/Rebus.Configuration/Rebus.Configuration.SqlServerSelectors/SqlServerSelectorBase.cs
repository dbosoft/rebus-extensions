using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Options = Rebus.Config.Options;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public abstract class SqlServerSelectorBase<TConfigurer> : GenericRebusSelectorBase<TConfigurer>
{
    
    protected SqlServerSelectorBase(IConfiguration configuration, 
        ILogger log) : base(configuration, log)
    {
    }

    public override string[] AcceptedConfigTypes => new[] { "mssql" };

    protected override void ConfigureByType(string busType, StandardConfigurer<TConfigurer> configurer)
    {
        switch (busType)
        {
            case "mssql":

                var connectionString = Configuration[$"{ConfigurationName}:connectionstring"];

                if (connectionString == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::connectionstring.");
                
                ConfigureSqlServer(configurer, connectionString);

                break;
        }
    }

    protected abstract void ConfigureSqlServer(StandardConfigurer<TConfigurer> configurer, string connectionString);
}