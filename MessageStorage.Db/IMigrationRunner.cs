using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MessageStorage.Db
{
    public interface IMigrationRunner
    {
        void Run(IEnumerable<IMigration> migrations, MessageStorageDbConfiguration messageStorageDbConfiguration);
    }

    public abstract class MigrationRunner : IMigrationRunner
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected MigrationRunner(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public void Run(IEnumerable<IMigration> migrations, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            migrations = migrations.OrderBy(m => m.GetType().Name);
            foreach (IMigration migration in migrations)
            {
                if (migration is IVersionedMigration versionedMigration)
                {
                    Run(versionedMigration, messageStorageDbConfiguration);
                }
                else
                {
                    Run(migration, messageStorageDbConfiguration);
                }
            }
        }

        private void Run(IVersionedMigration migration, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            using (IDbConnection dbConnection = _dbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    int lastVersionNumber = GetLastExecutedVersionNumber(messageStorageDbConfiguration);
                    if (migration.VersionNumber <= lastVersionNumber) return;

                    RunMigration(migration, dbTransaction, messageStorageDbConfiguration);
                    InsertMigrationToHistory(migration, dbTransaction, messageStorageDbConfiguration);
                    dbTransaction.Commit();
                }
            }
        }

        private void Run(IMigration migration, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            using (IDbConnection dbConnection = _dbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    RunMigration(migration, dbTransaction, messageStorageDbConfiguration);
                    dbTransaction.Commit();
                }
            }
        }

        private void RunMigration(IMigration migration, IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            (string commandText, IEnumerable<IDbDataAdapter> dbDataAdapters) = migration.Up(messageStorageDbConfiguration);
            using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbDataAdapters.ToList().ForEach(p => dbCommand.Parameters.Add(p));
                dbCommand.Transaction = dbTransaction;

                dbCommand.ExecuteNonQuery();
            }
        }

        protected abstract int GetLastExecutedVersionNumber(MessageStorageDbConfiguration messageStorageDbConfiguration);

        protected abstract void InsertMigrationToHistory(IVersionedMigration migration, IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration);
    }
}