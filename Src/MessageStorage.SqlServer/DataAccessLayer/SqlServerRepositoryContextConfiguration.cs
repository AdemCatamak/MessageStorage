namespace MessageStorage.SqlServer.DataAccessLayer;

public class SqlServerRepositoryContextConfiguration
{
    public string? Schema { get; private set; }
    public string ConnectionString { get; private set; }

    public SqlServerRepositoryContextConfiguration(string connectionString, string? schema = null)
    {
        ConnectionString = connectionString;
        Schema = schema;
    }
}