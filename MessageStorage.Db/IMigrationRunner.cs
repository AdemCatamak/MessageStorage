using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MessageStorage.Db
{
    public interface IMigrationRunner
    {
        void Run(IEnumerable<IMigration> migrations, IDbConnectionFactory dbConnectionFactory);
    }

    public abstract class MigrationRunner : IMigrationRunner
    {
        public void Run(IEnumerable<IMigration> migrations, IDbConnectionFactory dbConnectionFactory)
        {
            migrations = migrations.OrderBy(m => m.GetType().Name);
            foreach (IMigration migration in migrations)
            {
                if (migration is IVersionedMigration versionedMigration)
                {
                    Run(versionedMigration, dbConnectionFactory);
                }
                else
                {
                    Run(migration, dbConnectionFactory);
                }
            }
        }

        private void Run(IVersionedMigration migration, IDbConnectionFactory dbConnectionFactory)
        {
            using (IDbConnection dbConnection = dbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    int lastVersionNumber = GetLastExecutedVersionNumber(dbConnectionFactory);
                    if (migration.VersionNumber <= lastVersionNumber) return;

                    RunMigration(migration, dbTransaction, dbConnectionFactory.MessageStorageDbConfiguration);
                    InsertMigrationToHistory(migration, dbTransaction, dbConnectionFactory.MessageStorageDbConfiguration);
                    dbTransaction.Commit();
                }
            }
        }

        private void Run(IMigration migration, IDbConnectionFactory dbConnectionFactory)
        {
            using (IDbConnection dbConnection = dbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    RunMigration(migration, dbTransaction, dbConnectionFactory.MessageStorageDbConfiguration);
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

        protected abstract int GetLastExecutedVersionNumber(IDbConnectionFactory dbConnectionFactory);

        protected abstract void InsertMigrationToHistory(IVersionedMigration migration, IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration);
    }
}