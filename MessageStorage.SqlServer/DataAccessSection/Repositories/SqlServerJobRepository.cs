using System;
using System.Data;
using System.Globalization;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Models;

namespace MessageStorage.SqlServer.DataAccessSection.Repositories
{
    public class SqlServerJobRepository : BaseJobRepository
    {
        public SqlServerJobRepository(IDbTransaction dbTransaction, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : base(dbTransaction, messageStorageRepositoryContextConfiguration)
        {
        }

        public SqlServerJobRepository(IDbConnection dbConnection, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : base(dbConnection, messageStorageRepositoryContextConfiguration)
        {
        }

        protected override string GetJobCountStatement =>
            $"Select Count(JobId) From [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.JobTable}] " +
            "Where JobStatus = @JobStatus";

        protected override string GetJobStatement =>
            $@"
SELECT j.JobId, j.JobStatus, j.AssignedHandlerName, j.LastOperationInfo, j.LastOperationTime, j.MessageId, j.CreatedOn as JobCreatedOn,
        m.CreatedOn as MessageCreatedOn, m.SerializedPayload, m.PayloadClassName, m.PayloadClassFullName 
    FROM [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.JobTable}] j
    INNER JOIN [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.MessageTable}] m on j.MessageId = m.MessageId
WHERE J.JobId = @JobId
";

        protected override string UpdateJobStatusStatement =>
            $"Update [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.JobTable}] " +
            "Set JobStatus = @JobStatus, LastOperationTime = @LastOperationTime, LastOperationInfo = @LastOperationInfo " +
            "Where JobId = @JobId";

        protected override string AddStatement =>
            $"Insert Into [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.JobTable}] " +
            "(JobId, MessageId, AssignedHandlerName, JobStatus, LastOperationTime, LastOperationInfo, CreatedOn) " +
            "Values " +
            "(@JobId, @MessageId, @AssignedHandlerName, @JobStatus, @LastOperationTime, @LastOperationInfo, @CreatedOn)";

        protected override string SetFirstWaitingJobToInProgressStatement =>
            $@"
DECLARE @Updated table( [JobId] nvarchar(255))

UPDATE [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.JobTable}] SET JobStatus = {(int) JobStatus.InProgress}, LastOperationTime = '{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}'
OUTPUT INSERTED.JobId
INTO @Updated
WHERE  JobId = 
(
    SELECT TOP 1 JobId 
    FROM [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.JobTable}] WITH (UPDLOCK)
    WHERE JobStatus = {(int) JobStatus.Waiting}
    ORDER  BY JobId
)

SELECT j.JobId, j.JobStatus, j.AssignedHandlerName, j.LastOperationInfo, j.LastOperationTime, j.MessageId, j.CreatedOn as JobCreatedOn,
        m.CreatedOn as MessageCreatedOn, m.SerializedPayload, m.PayloadClassName, m.PayloadClassFullName 
    FROM  @Updated u
    INNER JOIN [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.JobTable}] j on j.JobId = u.JobId
    INNER JOIN [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.MessageTable}] m on j.MessageId = m.MessageId
";
    }
}