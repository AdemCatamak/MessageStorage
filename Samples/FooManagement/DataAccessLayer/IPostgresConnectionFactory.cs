using Npgsql;

namespace FooManagement.DataAccessLayer;

public interface IPostgresConnectionFactory
{
    Task<NpgsqlConnection> CreateAsync(CancellationToken cancellationToken);
}

public class PostgresConnectionFactory : IPostgresConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<NpgsqlConnection> CreateAsync(CancellationToken cancellationToken)
    {
        var npgsqlConnection = new NpgsqlConnection(_connectionString);
        await npgsqlConnection.OpenAsync(cancellationToken);
        return npgsqlConnection;
    }
}