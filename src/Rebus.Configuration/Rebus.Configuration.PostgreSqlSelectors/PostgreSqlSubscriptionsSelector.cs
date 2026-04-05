using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Subscriptions;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class PostgreSqlSubscriptionsSelector : PostgreSqlSelectorBase<ISubscriptionStorage>
{
    private readonly IOptions<PostgreSqlSubscriptionsStoreOptions> _options;

    public PostgreSqlSubscriptionsSelector(IOptions<PostgreSqlSubscriptionsStoreOptions> options, IConfiguration configuration, ILogger log) : base(configuration, log)
    {
        _options = options;
    }

    public override string ConfigurationName => "store";
    protected override void ConfigurePostgreSql(StandardConfigurer<ISubscriptionStorage> configurer, string connectionString)
    {
        configurer.StoreInPostgres(connectionString, _options.Value.TableName ?? "Subscriptions",
            isCentralized: _options.Value.IsCentralized ?? false,
            automaticallyCreateTables: _options.Value.AutomaticallyCreateTables ?? false);
    }
}
