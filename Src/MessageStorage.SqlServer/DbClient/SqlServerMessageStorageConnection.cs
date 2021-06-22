using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.DbClient
{
    public class SqlServerMessageStorageConnection : ISqlServerMessageStorageConnection
    {
        private readonly bool _connectionBorrowed;
        private readonly SqlConnection _sqlConnection;

        public SqlServerMessageStorageConnection(string connectionString)
        {
            _connectionBorrowed = false;
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public SqlServerMessageStorageConnection(SqlConnection connection)
        {
            _connectionBorrowed = true;
            _sqlConnection = connection;
        }

        public void Dispose()
        {
            if (!_connectionBorrowed)
            {
                _sqlConnection?.Dispose();
            }
        }

        public IMessageStorageTransaction BeginTransaction()
        {
            SqlTransaction sqlTransaction = _sqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            SqlServerMessageStorageTransaction transaction = new SqlServerMessageStorageTransaction(this, sqlTransaction, false);
            return transaction;
        }

        public Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
            return _sqlConnection.ExecuteAsync(commandDefinition);
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
            var result = await _sqlConnection.QueryAsync(commandDefinition);
            return result;
        }
    }
}