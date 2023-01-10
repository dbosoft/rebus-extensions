using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.DataBus;

namespace Dbosoft.Rebus.Configuration;

public class SqlServerDataBusSelector : SqlServerSelectorBase<IDataBusStorage>
{
    private readonly IOptions<SqlServerDataBusOptions> _options;

    public SqlServerDataBusSelector(IOptions<SqlServerDataBusOptions> options, IConfiguration configuration, ILogger log)
        : base(configuration, log)
    {
        _options = options;
    }

    public override string ConfigurationName => "databus";
    protected override void ConfigureSqlServer(StandardConfigurer<IDataBusStorage> configurer, string connectionString)
    {
        configurer.StoreInSqlServer(connectionString,
            _options.Value.TableName, _options.Value.AutomaticallyCreateTables);
    }
}