using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Timeouts;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class PostgreSqlTimeoutsSelector: PostgreSqlSelectorBase<ITimeoutManager>
{
    private readonly IOptions<PostgreSqlTimeoutsStoreOptions> _options;

    public PostgreSqlTimeoutsSelector(IOptions<PostgreSqlTimeoutsStoreOptions> options, IConfiguration configuration, ILogger log) : base(configuration, log)
    {
        _options = options;
    }

    public override string ConfigurationName => "store";
    protected override void ConfigurePostgreSql(StandardConfigurer<ITimeoutManager> configurer, string connectionString)
    {
        configurer.StoreInPostgres(connectionString,
            _options.Value.TableName ?? "Timeouts",
            _options.Value.AutomaticallyCreateTables ?? false);
    }
}
