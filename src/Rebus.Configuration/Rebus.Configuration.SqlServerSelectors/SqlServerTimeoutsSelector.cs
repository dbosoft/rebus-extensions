﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Timeouts;

namespace Dbosoft.Rebus.Configuration;

public class SqlServerTimeoutsSelector: SqlServerSelectorBase<ITimeoutManager>, IRebusTimeoutConfigurer
{
    private readonly IOptions<SqlServerTimeoutsStoreOptions> _options;

    public SqlServerTimeoutsSelector(IOptions<SqlServerTimeoutsStoreOptions> options, IConfiguration configuration, ILogger log) : base(configuration, log)
    {
        _options = options;
    }

    public override string ConfigurationName => "store";
    protected override void ConfigureSqlServer(StandardConfigurer<ITimeoutManager> configurer, string connectionString)
    {
        configurer.StoreInSqlServer(connectionString, _options.Value.TableName,_options.Value.AutomaticallyCreateTables );
    }
}