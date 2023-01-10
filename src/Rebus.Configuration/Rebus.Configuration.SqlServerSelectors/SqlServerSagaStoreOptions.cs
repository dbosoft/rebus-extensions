namespace Dbosoft.Rebus.Configuration;

public class SqlServerSagaStoreOptions
{
    public string DataTableName { get; set; }
    public string IndexTableName { get; set; }

    public bool AutomaticallyCreateTables { get; set; }
}