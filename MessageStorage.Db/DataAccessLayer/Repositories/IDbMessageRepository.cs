using System.Collections.Generic;
using System.Data;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db.DataAccessLayer.QueryBuilders;

namespace MessageStorage.Db.DataAccessLayer.Repositories
{
    public interface IDbMessageRepository : IDbRepository<Message>, IMessageRepository
    {
    }

    public class DbMessageRepository : IDbMessageRepository
    {
        private readonly IMessageQueryBuilder _messageQueryBuilder;
        private readonly IDbAdaptor _dbAdaptor;
        private readonly MessageStorageDbConfiguration _messageStorageDbConfiguration;

        public DbMessageRepository(IMessageQueryBuilder messageQueryBuilder, IDbAdaptor dbAdaptor, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            _messageQueryBuilder = messageQueryBuilder;
            _dbAdaptor = dbAdaptor;
            _messageStorageDbConfiguration = messageStorageDbConfiguration;
        }

        public void Add(Message entity)
        {
            using (IDbConnection dbConnection = _dbAdaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    Add(entity, dbTransaction);
                    dbTransaction.Commit();
                }
            }
        }

        public void Add(Message entity, IDbTransaction dbTransaction)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = _messageQueryBuilder.Add(entity);
            using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbCommand.Transaction = dbTransaction;
                foreach (IDbDataParameter dbDataParameter in dbDataParameters)
                {
                    dbCommand.Parameters.Add(dbDataParameter);
                }

                dbCommand.ExecuteNonQuery();
            }
        }
    }
}