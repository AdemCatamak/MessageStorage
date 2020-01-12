using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0006_CreateTable_Job : IVersionedMigration
    {
        public (string commandText, IEnumerable<IDbDataAdapter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
CREATE TABLE [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] (
    Id bigint NOT NULL PRIMARY KEY identity(1,1),
    MessageId bigint not null,
    AssignedHandlerName nvarchar(MAX) not null,
    JobStatus int not null,
    LastOperationInfo nvarchar(MAX),
    LastOperationTime DATETIME2(3) not null,
    
);";
            return (commandText, new List<IDbDataAdapter>());
        }

        public (string commandText, IEnumerable<IDbDataAdapter>) Down(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $"IF OBJECT_ID('{messageStorageDbConfiguration.Schema}.{TableNames.JobTable}', 'U') IS NOT NULL DROP TABLE {messageStorageDbConfiguration.Schema}.{TableNames.JobTable}; ";
            return (commandText, new List<IDbDataAdapter>());
        }

        public int VersionNumber => 6;
    }
}