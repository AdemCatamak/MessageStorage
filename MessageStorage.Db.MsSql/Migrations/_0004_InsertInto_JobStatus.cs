using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0004_InsertInto_JobStatus : IVersionedMigration
    {
        public (string commandText, IEnumerable<IDbDataAdapter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.Waiting},'{JobStatuses.Waiting.ToString()}' )
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.InProgress},'{JobStatuses.InProgress.ToString()}' )
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.Done},'{JobStatuses.Done.ToString()}' )
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.Failed},'{JobStatuses.Failed.ToString()}' )
   ";
            return (commandText, new List<IDbDataAdapter>());
        }

        public (string commandText, IEnumerable<IDbDataAdapter>) Down(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
DELETE FROM [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] WHERE Id in ({(int) JobStatuses.Waiting}, {(int) JobStatuses.InProgress}, {(int) JobStatuses.Done}, {(int) JobStatuses.Failed})
";
            return (commandText, new List<IDbDataAdapter>());
        }

        public int VersionNumber => 4;
    }
}