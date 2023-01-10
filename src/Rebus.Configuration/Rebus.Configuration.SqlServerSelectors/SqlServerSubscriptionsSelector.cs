using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Subscriptions;

namespace Dbosoft.Rebus.Configuration;

public class SqlServerSubscriptionsSelector : SqlServerSelectorBase<ISubscriptionStorage>
{
    private readonly IOptions<SqlServerSubscriptionsStoreOptions> _options;

    public SqlServerSubscriptionsSelector(IOptions<SqlServerSubscriptionsStoreOptions> options, IConfiguration configuration, ILogger log) : base(configuration, log)
    {
        _options = options;
    }

    public override string ConfigurationName => "store";
    protected override void ConfigureSqlServer(StandardConfigurer<ISubscriptionStorage> configurer, string connectionString)
    {
        configurer.StoreInSqlServer(connectionString, _options.Value.TableName, 
            isCentralized: _options.Value.IsCentralized,
            automaticallyCreateTables: _options.Value.AutomaticallyCreateTables);
    }
}