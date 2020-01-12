using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace MessageStorage.Db.MsSql
{
    public class MsSqlMigrationRunner : MigrationRunner, IMsSqlMigrationRunner
    {
        private readonly IMsSqlDbConnectionFactory _msSqlDbConnectionFactory;

        public MsSqlMigrationRunner(IMsSqlDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
        {
            _msSqlDbConnectionFactory = dbConnectionFactory;
        }


        protected override int GetLastExecutedVersionNumber(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            using (IDbConnection dbConnection = _msSqlDbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbCommand dbCommand = dbConnection.CreateCommand())
                {
                    string commandText = $"SELECT MAX(VersionNumber) FROM [{messageStorageDbConfiguration.Schema}].[{TableNames.VersionHistoryTable}]";
                    dbCommand.CommandText = commandText;
                    var result = dbCommand.ExecuteScalar();
                    int maxVersionNumber = int.Parse(result is DBNull ? "0" : result.ToString());
                    return maxVersionNumber;
                }
            }
        }

        protected override void InsertMigrationToHistory(IVersionedMigration migration, IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            int versionNumber = migration.VersionNumber;
            string migrationName = migration.GetType().Name;
            DateTime executionTime = DateTime.UtcNow;

            using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter("@VersionNumber", versionNumber)
                                  {
                                      SourceColumn = "VersionNumber"
                                  });
                sqlParameters.Add(new SqlParameter("@VersionName", migrationName)
                                  {
                                      SourceColumn = "VersionName"
                                  });
                sqlParameters.Add(new SqlParameter("@ExecutionTime", executionTime)
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