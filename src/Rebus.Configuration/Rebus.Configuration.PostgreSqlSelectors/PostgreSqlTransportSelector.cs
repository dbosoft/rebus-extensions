using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Transport;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class PostgreSqlTransportSelector : RebusTransportSelectorBase
{
    private readonly IOptions<PostgreSqlTransportOptions> _options;

    public PostgreSqlTransportSelector(IOptions<PostgreSqlTransportOptions> options, IConfiguration configuration, ILogger log) : base(configuration, log)
    {
        _options = options;
    }

    public override string[] AcceptedConfigTypes => new[] { "postgresql" };

    protected override void ConfigureBusType(string busType, string queueName, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "postgresql":
                var connectionString = Configuration[$"{ConfigurationName}:connectionstring"];

                if (connectionString == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::connectionstring.");

                configurer.UsePostgreSql(connectionString, _options.Value.TableName ?? "Messages", queueName);
                return;
        }
    }

    protected override void ConfigureBusTypeAsOneWayClient(string busType, StandardConfigurer<ITransport> configurer)
    {
        switch (busType)
        {
            case "postgresql":
                var connectionString = Configuration[$"{ConfigurationName}:connectionstring"];

                if (connectionString == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::connectionstring.");

                configurer.UsePostgreSqlAsOneWayClient(connectionString, _options.Value.TableName ?? "Messages");
                return;
        }
    }

}
