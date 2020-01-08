using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using MessageStorage.Db.DbStorageAdaptorSection;
using MessageStorage.Db.Exceptions;

namespace MessageStorage.Db.MsSql
{
    public class MsSqlDbStorageAdaptor : DbStorageAdaptor, IMsSqlDbStorageAdaptor
    {
        public MsSqlDbStorageAdaptor(IMsSqlDbConnectionFactory dbConnectionFactory, MessageStorageDbConfiguration messageStorageDbConfiguration) : base(dbConnectionFactory, messageStorageDbConfiguration)
        {
        }

        protected override (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Message message, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@TraceId", message.TraceId)
                              {
                                  SourceColumn = "TraceId"
                              });
            sqlParameters.Add(new SqlParameter("@CreatedOn", message.CreatedOn)
                              {
                                  SourceColumn = "CreatedOn"
                              });
            sqlParameters.Add(new SqlParameter("@SerializedPayload", message.SerializedPayload)
                              {
                                  SourceColumn = "SerializedPayload"
                              });
            sqlParameters.Add(new SqlParameter("@PayloadClassNamespace", message.PayloadClassNamespace)
                              {
                                  SourceColumn = "PayloadClassNamespace"
                              });
            sqlParameters.Add(new SqlParameter("@PayloadClassName", message.PayloadClassName)
                              {
                                  SourceColumn = "PayloadClassName"
                              });

            string columns = string.Join(",", sqlParameters.Select(p => $"[{p.SourceColumn}]"));
            string parameterNames = string.Join(",", sqlParameters.Select(p => $"[{p.ParameterName}]"));
            string commandText = $"INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.MessageStorageTable}] {columns} VALUES ({parameterNames}) SELECT SCOPE_IDENTITY()";

            return (commandText, sqlParameters);
        }

        protected override (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Job job, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@JobStatus", job.JobStatus)
                              {
                                  SourceColumn = "JobStatus"
                              });
            sqlParameters.Add(new SqlParameter("@AssignedAssignedHandlerName", job.AssignedAssignedHandlerName)
                              {
                                  SourceColumn = "AssignedAssignedHandlerName"
                              });
            sqlParameters.Add(new SqlParameter("@LastOperationInfo", job.LastOperationInfo)
                              {
                                  SourceColumn = "LastOperationInfo"
                              });
            sqlParameters.Add(new SqlParameter("@LastOperationTime", job.LastOperationTime)
                              {
                                  SourceColumn = "LastOperationTime"
                              });
            sqlParameters.Add(new SqlParameter("@MessageId", job.MessageId)
                              {
                                  SourceColumn = "MessageId"
                              });

            string columns = string.Join(",", sqlParameters.Select(p => $"[{p.SourceColumn}]"));
            string parameterNames = string.Join(",", sqlParameters.Select(p => $"[{p.ParameterName}]"));
            string commandText = $"INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] {columns} VALUES ({parameterNames}) SELECT SCOPE_IDENTITY()";

            return (commandText, sqlParameters);
        }

        protected override (string, IEnumerable<IDbDataParameter>) PrepareUpdateCommand(Job job, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@JobStatus", job.JobStatus)
                              {
                                  SourceColumn = "JobStatus"
                              });
            sqlParameters.Add(new SqlParameter("@AssignedAssignedHandlerName", job.AssignedAssignedHandlerName)
                              {
                                  SourceColumn = "AssignedAssignedHandlerName"
                              });
            sqlParameters.Add(new SqlParameter("@LastOperationInfo", job.LastOperationInfo)
                              {
                                  SourceColumn = "LastOperationInfo"
                              });
            sqlParameters.Add(new SqlParameter("@LastOperationTime", job.LastOperationTime)
                              {
                                  SourceColumn = "LastOperationTime"
                              });
            sqlParameters.Add(new SqlParameter("@MessageId", job.MessageId)
                              {
                                  SourceColumn = "MessageId"
                              });

            var idParameter = new SqlParameter("@Id", job.Id)
                              {
                                  SourceColumn = "Id"
                              };

            string setScript = string.Join(", ", sqlParameters.Select(p => $"[{p.SourceColumn}] = {p.ParameterName}"));
            string commandText = $"UPDATE [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] SET {setScript} WHERE [{idParameter.SourceColumn}] = {idParameter.ParameterName}";

            return (commandText, sqlParameters);
        }

        protected override (string, IEnumerable<IDbDataParameter>) SetFirstWaitingJobToInProgressCommand(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var jobStatusParameter = new SqlParameter("@JobStatus", (int) JobStatuses.Waiting)
                                     {
                                         SourceColumn = "JobStatus"
                                     };

            string commandText = $"DECLARE @Updated table( [JobId] int){Environment.NewLine}" +
                                 $"UPDATE [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] SET [{jobStatusParameter.ParameterName}] = {(int) JobStatuses.InProgress} OUTPUT inserted.*{Environment.NewLine}" +
                                 $"INTO @Updated{Environment.NewLine}" +
                                 $"WHERE [{jobStatusParameter.ParameterName}] = {jobStatusParameter.ParameterName}{Environment.NewLine}" +
                                 $"ORDER BY Id{Environment.NewLine}" +
                                 $"OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY{Environment.NewLine}" +
                                 $"{Environment.NewLine}" +
                                 $"SELECT j.Id, j.JobStatus, j.AssignedAssignedHandlerName, j.LastOperationInfo, j.LastOperationTime, j.MessageId,{Environment.NewLine}" +
                                 $"m.CreatedOn, m.SerializedPayload, m.TraceId, m.PayloadClassNamespace ,m.PayloadClassName {Environment.NewLine}" +
                                 $"FROM  @Updated j{Environment.NewLine}" +
                                 $"INNER JOIN [{messageStorageDbConfiguration.Schema}].{TableNames.MessageStorageTable} m on j.MessageId = m.Id";

            return (commandText, new List<IDbDataParameter> {jobStatusParameter});
        }

        protected override IDataAdapter GetDataAdaptor(IDbCommand dbCommand)
        {
            if (!(dbCommand is SqlCommand sqlCommand))
                throw new ArgumentException();

            return new SqlDataAdapter(sqlCommand);
        }

        protected override IEnumerable<Job> MapData(DataRowCollection dataRowCollection)
        {
            var jobs = new List<Job>();
            foreach (DataRow dataRow in dataRowCollection)
            {
                Message message = MapDataToMessage(dataRow);
                Job job = MapDataToJob(dataRow, message);
                jobs.Add(job);
            }

            return jobs;
        }

        private Job MapDataToJob(DataRow dataRow, Message message)
        {
            const string idColumnName = "Id";
            const string assignedHandlerNameColumnName = "AssignedHandlerName";
            const string jobStatusColumnName = "JobStatus";
            const string lastOperationTimeColumnName = "LastOperationTime";
            const string lastOperationInfoColumnName = "LastOperationInfo";

            if (!(dataRow[idColumnName] is Guid id))
                throw new DbGetOperationException($"{dataRow[idColumnName]} could not map");

            if (!(dataRow[assignedHandlerNameColumnName] is string assignedHandlerName))
                throw new DbGetOperationException($"{dataRow[assignedHandlerNameColumnName]} could not map");

            if (!(dataRow[jobStatusColumnName] is int jobStatus) || !(Enum.IsDefined(typeof(JobStatuses), jobStatus)))
                throw new DbGetOperationException($"{dataRow[jobStatusColumnName]} could not map");

            if (!(dataRow[lastOperationTimeColumnName] is DateTime lastOperationTime))
                throw new DbGetOperationException($"{dataRow[lastOperationTimeColumnName]} could not map");

            if (!(dataRow[lastOperationInfoColumnName] is string lastOperationInfo))
                throw new DbGetOperationException($"{dataRow[lastOperationInfoColumnName]} could not map");

            var job = new Job(id, message, assignedHandlerName, (JobStatuses) jobStatus, lastOperationTime, lastOperationInfo);

            return job;
        }

        private Message MapDataToMessage(DataRow dataRow)
        {
            const string messageIdColumnName = "MessageId";
            const string traceIdColumnName = "TraceId";
            const string serializedPayloadColumnName = "SerializedPayload";
            const string createdOnColumnName = "CreatedOn";
            const string payloadClassNamespaceColumnName = "PayloadClassNamespace";
            const string payloadClassNameColumnName = "PayloadClassName";

            if (!(dataRow[messageIdColumnName] is Guid id))
                throw new DbGetOperationException($"{dataRow[messageIdColumnName]} could not map");

            if (!(dataRow[traceIdColumnName] is string traceId))
                throw new DbGetOperationException($"{dataRow[traceIdColumnName]} could not map");

            if (!(dataRow[serializedPayloadColumnName] is string serializedPayload))
                throw new DbGetOperationException($"{dataRow[serializedPayloadColumnName]} could not map");

            if (!(dataRow[payloadClassNamespaceColumnName] is string payloadClassNamespace))
                throw new DbGetOperationException($"{dataRow[payloadClassNamespaceColumnName]} could not map");

            if (!(dataRow[payloadClassNameColumnName] is string payloadClassName))
                throw new DbGetOperationException($"{dataRow[payloadClassNameColumnName]} could not map");

            if (!(dataRow[createdOnColumnName] is DateTime createOn))
                throw new DbGetOperationException($"{dataRow[createdOnColumnName]} could not map");

            var message = new Message(id, traceId, createOn, payloadClassName, payloadClassNamespace, serializedPayload);

            return message;
        }
    }
}