using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace MessageStorage.Db.MsSql
{
    public interface IMsSqlMigrationRunner : IMigrationRunner
    {
    }

    public class MsSqlMigrationRunner : MigrationRunner
    {
        public MsSqlMigrationRunner(IDbAdaptor dbAdaptor) : base(dbAdaptor)
        {
        }

        protected override bool MigrationExecutedBefore(IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration, string versionName)
        {
            var versionNameParameter = new SqlParameter("@VersionName", versionName)
                                       {
                                           SourceColumn = "VersionName"
                                       };
            string commandText = $"SELECT Count(*) FROM [{messageStorageDbConfiguration.Schema}].[{TableNames.VersionHistoryTable}] WHERE {versionNameParameter.SourceColumn} = {versionNameParameter.ParameterName}";
            using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
            {
                dbCommand.Parameters.Add(versionNameParameter);
                dbCommand.Transaction = dbTransaction;
                dbCommand.CommandText = commandText;
                var exist = (int) dbCommand.ExecuteScalar();
                bool result = exist > 0;
                return result;
            }
        }

        protected override void InsertMigrationToHistory(IOneTimeMigration migration, IDbTransaction dbTransaction, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string migrationName = migration.GetType().Name;
            DateTime executionTime = DateTime.UtcNow;

            using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
            {
                var sqlParameters = new List<SqlParameter>();

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