using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Configuration;

public class SqlServerSagaStoreSelector : SqlServerSelectorBase<ISagaStorage>
{
    private readonly IOptions<SqlServerSagaStoreOptions> _options;

    public SqlServerSagaStoreSelector(IOptions<SqlServerSagaStoreOptions> options, IConfiguration configuration, ILogger log) 
        : base(configuration, log)
    {
        _options = options;
    }

    public override string ConfigurationName => "store";
    protected override void ConfigureSqlServer(StandardConfigurer<ISagaStorage> configurer, string connectionString)
    {
        configurer.StoreInSqlServer(connectionString,
            _options.Value.DataTableName, _options.Value.IndexTableName, _options.Value.AutomaticallyCreateTables);
    }
}