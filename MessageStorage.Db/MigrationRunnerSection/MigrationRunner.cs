using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MessageStorage.Db.MigrationRunnerSection
{
    public abstract class MigrationRunner : IMigrationRunner
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected MigrationRunner(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public void Run(IEnumerable<IMigration> migrations, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            int lastVersionNumber = GetLastExecutedVersionNumber(messageStorageDbConfiguration);
            foreach (IMigration migration in migrations)
            {
                if (migration is IVersionedMigration versionedMigration)
                {
                    if (versionedMigration.VersionNumber <= lastVersionNumber) continue;
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
                using (var dbTransaction = dbConnection.BeginTransaction())
                {
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
                using (var dbTransaction = dbConnection.BeginTransaction())
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