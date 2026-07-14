using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Injection;
using Rebus.Logging;
using Rebus.Sagas;
using Rebus.SqlServer;
using Rebus.SqlServer.Sagas;
using Rebus.SqlServer.Sagas.Serialization;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
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
        var dataTableName = _options.Value.DataTableName ?? "SagaData";
        var indexTableName = _options.Value.IndexTableName ?? "SagaIndex";
        var automaticallyCreateTables = _options.Value.AutomaticallyCreateTables ?? false;

        // Register SqlServerSagaStorage explicitly (rather than via StoreInSqlServer) so
        // we can pin a HASHED saga-type naming strategy. Rebus's default
        // LegacySagaTypeNamingStrategy derives the SagaIndex.saga_type from the saga-data
        // type's SHORT name (Type.Name), which drops generic arguments — so every closed
        // generic of the same open type maps to the SAME saga_type (e.g. Wrapper<A> and
        // Wrapper<B> both become "Wrapper`1"). When a message (such as a shared task
        // status event) is handled by several such saga types, the correlation lookup
        // returns a row belonging to the WRONG closed generic and deserialization fails
        // — the operation dies with a NullReferenceException deep in the saga storage.
        // Sha512SagaTypeNamingStrategy hashes the FULL type name, giving each closed
        // generic a distinct saga_type. (PostgreSql's saga storage keys on the full name
        // already, so its selector needs no equivalent change.)
        configurer.Register(context => BuildSagaStorage(
            connectionString, dataTableName, indexTableName, automaticallyCreateTables,
            context.Get<IRebusLoggerFactory>()));
    }

    /// <summary>
    /// Builds the SQL Server saga storage with the hashed
    /// <see cref="Sha512SagaTypeNamingStrategy"/> (see <see cref="ConfigureSqlServer"/>
    /// for why the default Legacy strategy must not be used). Internal so a test can
    /// assert the naming strategy without a live SQL Server: with
    /// <paramref name="automaticallyCreateTables"/> false the storage is constructed
    /// but never opens a connection.
    /// </summary>
    internal static SqlServerSagaStorage BuildSagaStorage(
        string connectionString,
        string dataTableName,
        string indexTableName,
        bool automaticallyCreateTables,
        IRebusLoggerFactory rebusLoggerFactory)
    {
        var connectionProvider = new DbConnectionProvider(connectionString, rebusLoggerFactory);

        var sagaStorage = new SqlServerSagaStorage(
            connectionProvider,
            dataTableName,
            indexTableName,
            rebusLoggerFactory,
            new Sha512SagaTypeNamingStrategy(),
            new DefaultSagaSerializer());

        if (automaticallyCreateTables)
            sagaStorage.EnsureTablesAreCreated();

        return sagaStorage;
    }
}
