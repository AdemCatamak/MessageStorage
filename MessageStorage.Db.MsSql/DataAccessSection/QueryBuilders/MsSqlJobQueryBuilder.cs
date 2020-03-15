using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.Db.DataAccessLayer.QueryBuilders;
using MessageStorage.Db.Exceptions;
using Microsoft.Data.SqlClient;

namespace MessageStorage.Db.MsSql.DataAccessSection.QueryBuilders
{
    public class MsSqlJobQueryBuilder : IJobQueryBuilder
    {
        private readonly MessageStorageDbConfiguration _messageStorageDbConfiguration;

        public MsSqlJobQueryBuilder(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            _messageStorageDbConfiguration = messageStorageDbConfiguration;
        }

        public (string, IEnumerable<IDbDataParameter>) Add(Job job)
        {
            var sqlParameters = new List<SqlParameter>
                                {
                                    new SqlParameter("@JobId", job.JobId) {SourceColumn = "JobId"},
                                    new SqlParameter("@JobStatus", job.JobStatus) {SourceColumn = "JobStatus"},
                                    new SqlParameter("@AssignedHandlerName", job.AssignedHandlerName) {SourceColumn = "AssignedHandlerName"},
                                    new SqlParameter("@LastOperationInfo", (object) job.LastOperationInfo ?? DBNull.Value) {SourceColumn = "LastOperationInfo"},
                                    new SqlParameter("@LastOperationTime", job.LastOperationTime) {SourceColumn = "LastOperationTime"},
                                    new SqlParameter("@MessageId", job.MessageId) {SourceColumn = "MessageId"},
                                };

            string columns = string.Join(",", sqlParameters.Select(p => $"[{p.SourceColumn}]"));
            string parameterNames = string.Join(",", sqlParameters.Select(p => $"{p.ParameterName}"));
            string commandText = $"INSERT INTO [{_messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] ({columns}) VALUES ({parameterNames}) SELECT SCOPE_IDENTITY()";

            return (commandText, sqlParameters);
        }

        public (string, IEnumerable<IDbDataParameter>) SetFirstWaitingJobToInProgress()
        {
            var waitingJobParameter = new SqlParameter("@WaitingJobParameter", (int) JobStatuses.Waiting)
                                      {
                                          SourceColumn = "JobStatus"
                                      };

            var inProgressJobParameter = new SqlParameter("@InProgressJobParameter", (int) JobStatuses.InProgress)
                                         {
                                             SourceColumn = "JobStatus"
                                         };

            string commandText = $@"
DECLARE @Updated table( [JobId] nvarchar(255))

UPDATE [{_messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] SET [{inProgressJobParameter.SourceColumn}] = {inProgressJobParameter.ParameterName}
OUTPUT INSERTED.JobId
INTO @Updated
WHERE  JobId = 
(
    SELECT TOP 1 JobId 
    FROM [{_messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] WITH (UPDLOCK)
    WHERE {waitingJobParameter.SourceColumn} = {waitingJobParameter.ParameterName}
    ORDER  BY JobId
)
SELECT j.JobId, j.JobStatus, j.AssignedHandlerName, j.LastOperationInfo, j.LastOperationTime, j.MessageId,
m.CreatedOn, m.SerializedPayload, m.TraceId, m.PayloadClassNamespace ,m.PayloadClassName
    FROM  @Updated u
    INNER JOIN [{_messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] j on j.JobId = u.JobId
    INNER JOIN [{_messageStorageDbConfiguration.Schema}].[{TableNames.MessageTable}] m on j.MessageId = m.MessageId
";

            return (commandText, new List<IDbDataParameter> {waitingJobParameter, inProgressJobParameter});
        }

        public (string, IEnumerable<IDbDataParameter>) Update(Job job)
        {
            var sqlParameters = new List<SqlParameter>
                                {
                                    new SqlParameter("@JobStatus", job.JobStatus) {SourceColumn = "JobStatus"},
                                    new SqlParameter("@AssignedHandlerName", job.AssignedHandlerName) {SourceColumn = "AssignedHandlerName"},
                                    new SqlParameter("@LastOperationInfo", (object) job.LastOperationInfo ?? DBNull.Value) {SourceColumn = "LastOperationInfo"},
                                    new SqlParameter("@LastOperationTime", job.LastOperationTime) {SourceColumn = "LastOperationTime"},
                                    new SqlParameter("@MessageId", job.MessageId) {SourceColumn = "MessageId"}
                                };

            var idParameter = new SqlParameter("@JobId", job.JobId)
                              {
                                  SourceColumn = "JobId"
                              };

            string setScript = string.Join(", ", sqlParameters.Select(p => $"[{p.SourceColumn}] = {p.ParameterName}"));
            string commandText = $"UPDATE [{_messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] SET {setScript} WHERE [{idParameter.SourceColumn}] = {idParameter.ParameterName}";

            return (commandText, new List<IDbDataParameter>(sqlParameters)
                                 {
                                     idParameter
                                 });
        }

        public (string, IEnumerable<IDbDataParameter>) GetJobCountByStatus(JobStatuses jobStatus)
        {
            var jobStatusParameter = new SqlParameter("@JobStatus", (int) jobStatus)
                                     {
                                         SourceColumn = "JobStatus"
                                     };
            var sqlParameters = new List<SqlParameter> {jobStatusParameter};

            string commandText = $"SELECT COUNT(JobId) FROM [{_messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] WHERE {jobStatusParameter.SourceColumn} = {jobStatusParameter.ParameterName}";

            return (commandText, sqlParameters);
        }

        public IEnumerable<Job> MapData(DataRowCollection rows)
        {
            var jobs = new List<Job>();
            foreach (DataRow dataRow in rows)
            {
                Message message = MapDataToMessage(dataRow);
                Job job = MapDataToJob(dataRow, message);
                jobs.Add(job);
            }

            return jobs;
        }

        private static Job MapDataToJob(DataRow dataRow, Message message)
        {
            const string idColumnName = "JobId";
            const string assignedHandlerNameColumnName = "AssignedHandlerName";
            const string jobStatusColumnName = "JobStatus";
            const string lastOperationTimeColumnName = "LastOperationTime";
            const string lastOperationInfoColumnName = "LastOperationInfo";

            if (!(dataRow[idColumnName] is string jobId))
                throw new DbGetOperationException($"{dataRow[idColumnName]} could not map");

            if (!(dataRow[assignedHandlerNameColumnName] is string assignedHandlerName))
                throw new DbGetOperationException($"{dataRow[assignedHandlerNameColumnName]} could not map");

            if (!(dataRow[jobStatusColumnName] is int jobStatus) || !(Enum.IsDefined(typeof(JobStatuses), jobStatus)))
                throw new DbGetOperationException($"{dataRow[jobStatusColumnName]} could not map");

            if (!(dataRow[lastOperationTimeColumnName] is DateTime lastOperationTime))
                throw new DbGetOperationException($"{dataRow[lastOperationTimeColumnName]} could not map");

            string lastOperationInfo = null;
            switch (dataRow[lastOperationInfoColumnName])
            {
                case DBNull _:
                    break;
                case string x:
                    lastOperationInfo = x;
                    break;
                default:
                    throw new DbGetOperationException($"{dataRow[lastOperationInfoColumnName]} could not map");
            }

            var job = new Job(jobId, message, assignedHandlerName, (JobStatuses) jobStatus, lastOperationTime, lastOperationInfo);

            return job;
        }

        private static Message MapDataToMessage(DataRow dataRow)
        {
            const string messageIdColumnName = "MessageId";
            const string traceIdColumnName = "TraceId";
            const string serializedPayloadColumnName = "SerializedPayload";
            const string createdOnColumnName = "CreatedOn";
            const string payloadClassNamespaceColumnName = "PayloadClassNamespace";
            const string payloadClassNameColumnName = "PayloadClassName";

            if (!(dataRow[messageIdColumnName] is string messageId))
                throw new DbGetOperationException($"{dataRow[messageIdColumnName]} could not map");

            string traceId = null;
            switch (dataRow[traceIdColumnName])
            {
                case DBNull _:
                    break;
                case string x:
                    traceId = x;
                    break;
                default:
                    throw new DbGetOperationException($"{dataRow[traceIdColumnName]} could not map");
            }

            if (!(dataRow[serializedPayloadColumnName] is string serializedPayload))
                throw new DbGetOperationException($"{dataRow[serializedPayloadColumnName]} could not map");

            if (!(dataRow[payloadClassNamespaceColumnName] is string payloadClassNamespace))
                throw new DbGetOperationException($"{dataRow[payloadClassNamespaceColumnName]} could not map");

            if (!(dataRow[payloadClassNameColumnName] is string payloadClassName))
                throw new DbGetOperationException($"{dataRow[payloadClassNameColumnName]} could not map");

            if (!(dataRow[createdOnColumnName] is DateTime createOn))
                throw new DbGetOperationException($"{dataRow[createdOnColumnName]} could not map");

            var message = new Message(messageId, createOn, traceId, payloadClassName, payloadClassNamespace, serializedPayload);

            return message;
        }
    }
}