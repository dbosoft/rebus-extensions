namespace Dbosoft.Rebus.Configuration;

public class PostgreSqlSagaStoreOptions
{
    public string? DataTableName { get; set; }
    public string? IndexTableName { get; set; }

    public bool? AutomaticallyCreateTables { get; set; }
}
