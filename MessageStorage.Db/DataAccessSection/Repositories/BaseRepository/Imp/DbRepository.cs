using System.Data;
using System.Runtime.CompilerServices;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.Exceptions;
using MessageStorage.Models.Base;

namespace MessageStorage.Db.DataAccessSection.Repositories.BaseRepository.Imp
{
    public abstract class DbRepository< TEntity> : IDbRepository< TEntity>
        where TEntity : Entity
    {
        public DbRepositoryConfiguration DbRepositoryConfiguration { get; }
        protected readonly IDbConnection DbConnection;
        protected IDbTransaction DbTransaction;

        public bool HasTransaction => DbTransaction != null;

        protected DbRepository(IDbConnection dbConnection, DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            DbRepositoryConfiguration = dbRepositoryConfiguration;
            DbConnection = dbConnection;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UseTransaction(IDbTransaction dbTransaction)
        {
            if (DbTransaction != null)
            {
                throw new AnotherTransactionAssignedException();
            }

            DbTransaction = dbTransaction;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ClearTransaction()
        {
            DbTransaction = null;
        }

        public void Add(TEntity entity)
        {
            IDbConnection connectionThatUsed = DbTransaction?.Connection ?? DbConnection;

            using (IDbCommand dbCommand = connectionThatUsed.CreateCommand())
            {
                dbCommand.Transaction = DbTransaction;
                (string commandText, IDataParameter[] dataParameters) = PrepareAddCommand(entity);

                dbCommand.CommandText = commandText;
                foreach (IDataParameter dataParameter in dataParameters)
                {
                    dbCommand.Parameters.Add(dataParameter);
                }

                int affectedRowCount = dbCommand.ExecuteNonQuery();

                if (affectedRowCount != 1)
                {
                    throw new InsertFailedException($"[EntityId: {entity.Id}, EntityType: {typeof(TEntity).Name}] insert script return affected row count as {affectedRowCount}");
                }
            }
        }

        protected abstract (string, IDbDataParameter[]) PrepareAddCommand(TEntity entity);
    }
}