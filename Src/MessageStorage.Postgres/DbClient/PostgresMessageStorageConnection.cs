using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using Npgsql;

namespace MessageStorage.Postgres.DbClient
{
    public class PostgresMessageStorageConnection : IPostgresMessageStorageConnection
    {
        private readonly bool _connectionOwner;
        private readonly NpgsqlConnection _npgsqlConnection;

        public PostgresMessageStorageConnection(string connectionString)
        {
            _connectionOwner = true;
            _npgsqlConnection = new NpgsqlConnection(connectionString);
            _npgsqlConnection.Open();
        }

        public PostgresMessageStorageConnection(NpgsqlConnection npgsqlConnection)
        {
            _connectionOwner = false;
            _npgsqlConnection = npgsqlConnection;
        }

        public void Dispose()
        {
            if (_connectionOwner)
                _npgsqlConnection?.Dispose();
        }

        IMessageStorageTransaction IMessageStorageConnection.BeginTransaction()
        {
            return BeginTransaction();
        }

        public IPostgresMessageStorageTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            NpgsqlTransaction npgsqlTransaction = _npgsqlConnection.BeginTransaction(isolationLevel);
            PostgresMessageStorageTransaction transaction = new PostgresMessageStorageTransaction(this, npgsqlTransaction, false);
            return transaction;
        }


        public IPostgresMessageStorageTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
            return _npgsqlConnection.ExecuteAsync(commandDefinition);
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
            var result = await _npgsqlConnection.QueryAsync(commandDefinition);
            return result;
        }
    }
}