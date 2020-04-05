using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0004_InsertInto_JobStatus : IOneTimeMigration
    {
        public (string commandText, IEnumerable<IDbDataParameter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.Waiting},'{JobStatuses.Waiting.ToString()}' )
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.InProgress},'{JobStatuses.InProgress.ToString()}' )
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.Done},'{JobStatuses.Done.ToString()}' )
INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobStatusTable}] (Id, Description) VALUES ({(int) JobStatuses.Failed},'{JobStatuses.Failed.ToString()}' )
   ";
            return (commandText, new List<IDbDataParameter>());
        }
    }
}