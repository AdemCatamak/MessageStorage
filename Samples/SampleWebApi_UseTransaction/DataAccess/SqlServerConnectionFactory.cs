using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace SampleWebApi_UseTransaction.DataAccess;

public class SqlServerConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDb();
    }

    private void InitializeDb()
    {
        var script = @"
if not exists (select * from sys.tables t join sys.schemas s on (t.schema_id = s.schema_id) where s.name = 'use_transaction_schema' and t.name = 'accounts')
    create table use_transaction_schema.accounts (
        account_id uniqueidentifier not null primary key,
        email nvarchar(100) not null,
        created_on datetime not null
    );
";
        using IDbConnection connection = CreateConnection();
        connection.Execute(script);
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}