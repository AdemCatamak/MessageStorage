using Microsoft.Data.SqlClient;

namespace FooManagement.DataAccessLayer;

public interface ISqlConnectionFactory
{
    Task<SqlConnection> CreateAsync(CancellationToken cancellationToken);
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<SqlConnection> CreateAsync(CancellationToken cancellationToken)
    {
        var sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);
        return sqlConnection;
    }
}