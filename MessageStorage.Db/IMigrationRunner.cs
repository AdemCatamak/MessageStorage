using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MessageStorage.Db
{
    public interface IMigrationRunner
    {
        void Run<TMessageStorageDbConfiguration>(IEnumerable<IMigration> migrations, TMessageStorageDbConfiguration messageStorageDbConfiguration)
            where TMessageStorageDbConfiguration : MessageStorageDbConfiguration;
    }

    public abstract class MigrationRunner : IMigrationRunner
    {
        private readonly IDbAdaptor _dbAdaptor;

        protected MigrationRunner(IDbAdaptor dbAdaptor)
        {
            _dbAdaptor = dbAdaptor;
        }

        public void Run<TMessageStorageDbConfiguration>(IEnumerable<IMigration> migrations, TMessageStorageDbConfiguration messageStorageDbConfiguration)
            where TMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            migrations = migrations.OrderBy(m => m.GetType().Name);
            foreach (IMigration migration in migrations)
            {
                if (migration is IOneTimeMigration versionedMigration)
                {
                    Run(versionedMigration, messageStorageDbConfiguration);
                }
                else
                {
                    Run(migration, messageStorageDbConfiguration);
                }
            }
        }


        private void Run(IOneTimeMigration migration, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            using (IDbConnection dbConnection = _dbAdaptor.CreateConnection(messageStorageDbConfiguration.ConnectionStr))
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    bool executedBefore = MigrationExecutedBefore(dbTransaction, messageStorageDbConfiguration, migration.GetType().Name);
                    if (executedBefore) return;

                    RunMigration(migration, dbTransaction, messageStorageDbConfiguration);
                    InsertMigrationToHistory(migration, dbTransaction, messageStorageDbConfiguration);
                    dbTransaction.Commit();
                }
            }
        }

        private void Run(IMigration migration, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            using (IDbConnection dbConnection = _dbAdaptor.CreateConnection(messageStorageDbConfiguration.ConnectionStr))
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
            (string commandText, IEnumerable<IDbDataParameter> dbDataAdapters) = migration.Up(messageStorageDbConfiguration);
            using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbDataAdapters.ToList().ForEach(p => dbCommand.Parameters.Add(p));
                dbCommand.Transaction = dbTransaction;

                dbCommand.ExecuteNonQuery();
            }
        }

        protected abstract bool MigrationExecutedBefore(IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration, string versionName);

        protected abstract void InsertMigrationToHistory(IOneTimeMigration migration, IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration);
    }
}