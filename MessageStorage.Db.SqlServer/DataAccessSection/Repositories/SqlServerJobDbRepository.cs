using System;
using System.Data;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.Imp;
using MessageStorage.Db.Exceptions;
using MessageStorage.Models;
using Microsoft.Data.SqlClient;

namespace MessageStorage.Db.SqlServer.DataAccessSection.Repositories
{
    public class SqlServerJobDbRepository : JobDbRepository
    {
        public SqlServerJobDbRepository(IDbConnection dbConnection, DbRepositoryConfiguration dbRepositoryConfiguration)
            : base(dbConnection, dbRepositoryConfiguration)
        {
        }

        protected override (string, IDbDataParameter[]) PrepareAddCommand(Job entity)
        {
            string commandText = $"Insert Into [{DbRepositoryConfiguration.Schema}].[{TableNames.JobTable}] (JobId, CreatedOn, MessageId, AssignedHandlerName, JobStatus, LastOperationTime, LastOperationInfo) Values (@JobId, @CreatedOn, @MessageId, @AssignedHandlerName, @JobStatus, @LastOperationTime, @LastOperationInfo)";
            IDbDataParameter[] dataParameters =
            {
                new SqlParameter("@JobId", entity.Id),
                new SqlParameter("@CreatedOn", entity.CreatedOn),
                new SqlParameter("@MessageId", entity.Message.Id),
                new SqlParameter("@AssignedHandlerName", entity.AssignedHandlerName),
                new SqlParameter("@JobStatus", entity.JobStatus),
                new SqlParameter("@LastOperationTime", entity.LastOperationTime),
                new SqlParameter("@LastOperationInfo", entity.LastOperationInfo ?? (object) DBNull.Value),
            };

            return (commandText, dataParameters);
        }

        protected override (string, IDataParameter[]) PrepareUpdateCommand(Job job)
        {
            string commandText = $"Update [{DbRepositoryConfiguration.Schema}].[{TableNames.JobTable}] Set CreatedOn = @CreatedOn, MessageId = @MessageId, AssignedHandlerName = @AssignedHandlerName, JobStatus = @JobStatus, LastOperationTime = @LastOperationTime, LastOperationInfo = @LastOperationInfo Where JobId = @JobId";
            IDataParameter[] dataParameters =
            {
                new SqlParameter("JobId", job.Id),
                new SqlParameter("CreatedOn", job.CreatedOn),
                new SqlParameter("MessageId", job.Message.Id),
                new SqlParameter("AssignedHandlerName", job.AssignedHandlerName),
                new SqlParameter("JobStatus", job.JobStatus),
                new SqlParameter("LastOperationTime", job.LastOperationTime),
                new SqlParameter("LastOperationInfo", job.LastOperationInfo),
            };

            return (commandText, dataParameters);
        }

        protected override (string, IDataParameter[]) PrepareGetJobCountByStatusCommand(JobStatus jobStatus)
        {
            string commandText = $"Select Count(JobId) From [{DbRepositoryConfiguration.Schema}].[{TableNames.JobTable}] Where JobStatus = @JobStatus";
            IDataParameter[] dataParameters =
            {
                new SqlParameter("JobStatus", (int) jobStatus),
            };

            return (commandText, dataParameters);
        }

        protected override (string, IDataParameter[], Func<IDataReader, Job>) PrepareSetFirstWaitingJobToInProgressCommand()
        {
            string commandText = $@"
DECLARE @Updated table( [JobId] nvarchar(255))

UPDATE [{DbRepositoryConfiguration.Schema}].[{TableNames.JobTable}] SET JobStatus = {(int) JobStatus.InProgress}
OUTPUT INSERTED.JobId
INTO @Updated
WHERE  JobId = 
(
    SELECT TOP 1 JobId 
    FROM [{DbRepositoryConfiguration.Schema}].[{TableNames.JobTable}] WITH (UPDLOCK)
    WHERE JobStatus = {(int) JobStatus.Waiting}
    ORDER  BY JobId
)

SELECT j.JobId, j.JobStatus, j.AssignedHandlerName, j.LastOperationInfo, j.LastOperationTime, j.MessageId, j.CreatedOn as JobCreatedOn,
        m.CreatedOn as MessageCreatedOn, m.SerializedPayload, m.PayloadClassName, m.PayloadClassFullName 
    FROM  @Updated u
    INNER JOIN [{DbRepositoryConfiguration.Schema}].[{TableNames.JobTable}] j on j.JobId = u.JobId
    INNER JOIN [{DbRepositoryConfiguration.Schema}].[{TableNames.MessageTable}] m on j.MessageId = m.MessageId
";

            static Job BuildJob(IDataReader r)
            {
                static Message MapDataToMessage(IDataRecord dataRow)
                {
                    const string messageIdColumnName = "MessageId";
                    const string createdOnColumnName = "MessageCreatedOn";
                    const string serializedPayloadColumnName = "SerializedPayload";

                    if (!(dataRow[messageIdColumnName] is string messageId))
                        throw new DbColumnMapException(messageIdColumnName, nameof(String));

                    if (!(dataRow[createdOnColumnName] is DateTime createdOn))
                        throw new DbColumnMapException(createdOnColumnName, nameof(DateTime));

                    if (!(dataRow[serializedPayloadColumnName] is string serializedPayload))
                        throw new DbColumnMapException(serializedPayloadColumnName, nameof(String));

                    var m = new Message(messageId, createdOn, serializedPayload);

                    return m;
                }

                static Job MapDataToJob(IDataRecord dataRow, Message m)
                {
                    const string idColumnName = "JobId";
                    const string createdOnColumnName = "JobCreatedOn";
                    const string assignedHandlerNameColumnName = "AssignedHandlerName";
                    const string jobStatusColumnName = "JobStatus";
                    const string lastOperationTimeColumnName = "LastOperationTime";
                    const string lastOperationInfoColumnName = "LastOperationInfo";

                    if (!(dataRow[idColumnName] is string jobId))
                        throw new DbColumnMapException(idColumnName, nameof(String));

                    if (!(dataRow[createdOnColumnName] is DateTime createdOn))
                        throw new DbColumnMapException(createdOnColumnName, nameof(DateTime));

                    if (!(dataRow[assignedHandlerNameColumnName] is string assignedHandlerName))
                        throw new DbColumnMapException(assignedHandlerNameColumnName, nameof(String));

                    if (!(dataRow[jobStatusColumnName] is int jobStatus) || !Enum.IsDefined(typeof(JobStatus), jobStatus))
                        throw new DbColumnMapException(jobStatusColumnName, nameof(JobStatus));


                    if (!(dataRow[lastOperationTimeColumnName] is DateTime lastOperationTime))
                        throw new DbColumnMapException(lastOperationTimeColumnName, nameof(DateTime));

                    string lastOperationInfo = null;
                    switch (dataRow[lastOperationInfoColumnName])
                    {
                        case DBNull _:
                            break;
                        case string x:
                            lastOperationInfo = x;
                            break;
                        default:
                            throw new DbColumnMapException(lastOperationInfoColumnName, nameof(DateTime));
                    }


                    var j = new Job(jobId, createdOn, assignedHandlerName, (JobStatus) jobStatus, lastOperationTime, lastOperationInfo, m);

                    return j;
                }

                Message message = MapDataToMessage(r);
                Job job = MapDataToJob(r, message);
                return job;
            }

            return (commandText, new IDataParameter[] { }, BuildJob);
        }
    }
}