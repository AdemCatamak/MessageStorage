using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0003_CreateTable_JobStatus : IOneTimeMigration
    {
        public (string commandText, IEnumerable<IDbDataParameter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
CREATE TABLE [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (
    Id int NOT NULL PRIMARY KEY,
    Description nvarchar(MAX)
);";

            return (commandText, new List<IDbDataParameter>());
        }
    }
}