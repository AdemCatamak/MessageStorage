using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using MessageStorage.Db.MigrationRunnerSection;

namespace MessageStorage.Db.MsSql
{
    public class MsSqlMigrationRunner : MigrationRunner, IMsSqlMigrationRunner
    {
        private IMsSqlDbConnectionFactory _msSqlDbConnectionFactory;

        public MsSqlMigrationRunner(IMsSqlDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
        {
            _msSqlDbConnectionFactory = dbConnectionFactory;
        }


        protected override int GetLastExecutedVersionNumber(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            using (var dbConnection = _msSqlDbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    string commandText = $"SELECT MAX(VersionNumber) FROM [{messageStorageDbConfiguration.Schema}].[{TableNames.VersionHistoryTable}]";
                    dbCommand.CommandText = commandText;
                    int maxVersionNumber = int.Parse(dbCommand.ExecuteScalar().ToString());
                    return maxVersionNumber;
                }
            }
        }

        protected override void InsertMigrationToHistory(IVersionedMigration migration, IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            int versionNumber = migration.VersionNumber;
            string migrationName = migration.GetType().Name;
            DateTime executedTime = DateTime.UtcNow;

            using (var dbCommand = dbTransaction.Connection.CreateCommand())
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter("@VersionNumber", versionNumber)
                                  {
                                      SourceColumn = "VersionNumber"
                                  });
                sqlParameters.Add(new SqlParameter("@MigrationName", migrationName)
                                  {
                                      SourceColumn = "MigrationName"
                                  });
                sqlParameters.Add(new SqlParameter("@ExecutionTime", executedTime)
                                  {
                                      SourceColumn = "ExecutionTime"
                                  });

                string columns = string.Join(",", sqlParameters.Select(p => $"[{p.SourceColumn}]"));
                string parameters = string.Join(",", sqlParameters.Select(p => $"{p.ParameterName}"));

                string commandText = $"INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.VersionHistoryTable}] ({columns}) VALUES ({parameters})";

                dbCommand.CommandText = commandText;
                sqlParameters.ForEach(p => dbCommand.Parameters.Add(p));
                dbCommand.Transaction = dbTransaction;
                dbCommand.ExecuteNonQuery();
            }
        }
    }
}