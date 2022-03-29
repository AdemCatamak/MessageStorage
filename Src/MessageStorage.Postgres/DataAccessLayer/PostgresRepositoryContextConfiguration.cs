namespace MessageStorage.Postgres.DataAccessLayer;

public class PostgresRepositoryContextConfiguration
{
    public string? Schema { get; private set; }
    public string ConnectionString { get; private set; }

    public PostgresRepositoryContextConfiguration(string connectionString, string? schema = null)
    {
        ConnectionString = connectionString;
        Schema = schema;
    }
}