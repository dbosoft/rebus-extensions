namespace Dbosoft.Rebus.Configuration;

public class PostgreSqlSubscriptionsStoreOptions
{
    public string? TableName { get; set; }

    public bool? IsCentralized { get; set; }

    public bool? AutomaticallyCreateTables { get; set; }
}
