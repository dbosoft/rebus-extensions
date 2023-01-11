namespace Dbosoft.Rebus.Configuration;

public class SqlServerDataBusOptions
{
    public string? TableName { get; set; }

    public bool? AutomaticallyCreateTables { get; set; }
}