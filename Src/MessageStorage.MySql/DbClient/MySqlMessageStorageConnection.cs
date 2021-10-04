using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MySql.Data.MySqlClient;

namespace MessageStorage.MySql.DbClient
{
    public class MySqlMessageStorageConnection : IMySqlMessageStorageConnection
    {
        private readonly bool _connectionOwner;
        private readonly MySqlConnection _mySqlConnection;

        public MySqlMessageStorageConnection(string connectionString)
        {
            _connectionOwner = true;
            _mySqlConnection = new MySqlConnection(connectionString);
            _mySqlConnection.Open();
        }

        public MySqlMessageStorageConnection(MySqlConnection mysqlConnection)
        {
            _connectionOwner = false;
            _mySqlConnection = mysqlConnection;
        }

        public void Dispose()
        {
            if (_connectionOwner)
                _mySqlConnection?.Dispose();
        }

        IMessageStorageTransaction IMessageStorageConnection.BeginTransaction()
        {
            return BeginTransaction();
        }

        public IMySqlMessageStorageTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            MySqlTransaction mySqlTransaction = _mySqlConnection.BeginTransaction(isolationLevel);
            var transaction = new MySqlMessageStorageTransaction(this, mySqlTransaction, false);
            return transaction;
        }


        public IMySqlMessageStorageTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
            return _mySqlConnection.ExecuteAsync(commandDefinition);
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
            var result = await _mySqlConnection.QueryAsync(commandDefinition);
            return result;
        }
    }
}