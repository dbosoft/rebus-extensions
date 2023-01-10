using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Transport;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class SqlServerTransportSelector : RebusTransportSelectorBase
{

    public SqlServerTransportSelector(IConfiguration configuration, ILogger log) : base(configuration, log)
    {
    }

    public override string[] AcceptedConfigTypes => new[] { "mssql" };

    protected override void ConfigureBusType(string busType, string queueName, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "mssql":
                var connectionString = Configuration[$"{ConfigurationName}:connectionstring"];

                if (connectionString == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::connectionstring.");

                configurer.UseSqlServer(new SqlServerTransportOptions(connectionString), queueName);
                return;
        }
    }

    protected override void ConfigureBusTypeAsOneWayClient(string busType, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "mssql":
                var connectionString = Configuration[$"{ConfigurationName}:connectionstring"];

                if (connectionString == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::connectionstring.");

                configurer.UseSqlServerAsOneWayClient(new SqlServerTransportOptions(connectionString));
                return;
        }
    }



}