namespace Dbosoft.Rebus.Configuration;

public class PostgreSqlTimeoutsStoreOptions
{
    public string? TableName { get; set; }
    public bool? AutomaticallyCreateTables { get; set; }
}
