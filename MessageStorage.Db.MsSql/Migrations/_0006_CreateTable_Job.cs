using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0006_CreateTable_Job : IOneTimeMigration
    {
        public (string commandText, IEnumerable<IDbDataAdapter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
CREATE TABLE [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] (
    JobId nvarchar(255) NOT NULL PRIMARY KEY,
    MessageId nvarchar(255) not null,
    AssignedHandlerName nvarchar(MAX) not null,
    JobStatus int not null,
    LastOperationInfo nvarchar(MAX),
    LastOperationTime DATETIME2(3) not null,
    
);";
            return (commandText, new List<IDbDataAdapter>());
        }

        public int VersionNumber => 6;
    }
}