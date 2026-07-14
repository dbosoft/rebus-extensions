using System.Reflection;
using Rebus.Logging;
using Rebus.SqlServer.Sagas;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public class SqlServerSagaStoreSelectorTests
{
    [Fact]
    public void Builds_SqlServer_saga_storage_with_a_hashed_naming_strategy()
    {
        // AutomaticallyCreateTables = false → the storage is constructed but never opens
        // a connection, so this runs without a real SQL Server.
        var storage = SqlServerSagaStoreSelector.BuildSagaStorage(
            "Server=.;Database=Test;Trusted_Connection=True;",
            "SagaData", "SagaIndex",
            automaticallyCreateTables: false,
            new ConsoleLoggerFactory(false));

        // The default LegacySagaTypeNamingStrategy derives saga_type from the short type
        // name and drops generic arguments, colliding for closed generic saga data. The
        // selector must pin a hashed strategy instead.
        var strategy = typeof(SqlServerSagaStorage)
            .GetField("_sagaTypeNamingStrategy", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(storage);

        Assert.IsType<Sha512SagaTypeNamingStrategy>(strategy);
    }

    [Fact]
    public void Legacy_naming_collides_for_closed_generics_but_hashed_does_not()
    {
        // Two distinct closed generics of the same open type — the shape that collides
        // (e.g. SapIdSagaData<CreateGroups…> vs SapIdSagaData<CreateUsers…> in SAPId).
        var alpha = typeof(List<int>);
        var beta = typeof(List<string>);

        var legacy = new LegacySagaTypeNamingStrategy();
        var hashed = new Sha512SagaTypeNamingStrategy();

        // SqlServerSagaStorage.MaximumSagaDataTypeNameLength.
        const int maxLength = 40;

        // The bug: Legacy maps both closed generics to the SAME saga_type ("List`1").
        Assert.Equal(
            legacy.GetSagaTypeName(alpha, maxLength),
            legacy.GetSagaTypeName(beta, maxLength));

        // The fix: hashing the full type name keeps them distinct.
        Assert.NotEqual(
            hashed.GetSagaTypeName(alpha, maxLength),
            hashed.GetSagaTypeName(beta, maxLength));
    }
}
