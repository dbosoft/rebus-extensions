using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class PostgreSqlSagaStoreSelector : PostgreSqlSelectorBase<ISagaStorage>
{
    private readonly IOptions<PostgreSqlSagaStoreOptions> _options;

    public PostgreSqlSagaStoreSelector(IOptions<PostgreSqlSagaStoreOptions> options, IConfiguration configuration, ILogger log)
        : base(configuration, log)
    {
        _options = options;
    }

    public override string ConfigurationName => "store";
    protected override void ConfigurePostgreSql(StandardConfigurer<ISagaStorage> configurer, string connectionString)
    {
        configurer.StoreInPostgres(connectionString,
            _options.Value.DataTableName ?? "SagaData",
            _options.Value.IndexTableName ?? "SagaIndex",
            _options.Value.AutomaticallyCreateTables ?? false);
    }
}
