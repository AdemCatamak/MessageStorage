using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using MessageStorage.Db.Exceptions;
using MessageStorage.Db.MsSql.Migrations;
using MessageStorage.Exceptions;

namespace MessageStorage.Db.MsSql
{
    public class MsSqlDbStorageAdaptor : DbStorageAdaptor, IMsSqlDbStorageAdaptor, IMsSqlDbConnectionFactory
    {
        private readonly IMsSqlMigrationRunner _msSqlMigrationRunner;

        public MsSqlDbStorageAdaptor(IMsSqlMigrationRunner msSqlMigrationRunner)
        {
            _msSqlMigrationRunner = msSqlMigrationRunner;
        }

        public override void SetConfiguration(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            base.SetConfiguration(messageStorageDbConfiguration);
            _msSqlMigrationRunner.Run(GetMigrations(), this);
        }

        private IEnumerable<IMigration> GetMigrations()
        {
            Type migrationType = typeof(IMigration);
            IEnumerable<Type> migrationClasses = typeof(_0001_CreateSchema).Assembly.GetTypes()
                                                                           .Where(p => migrationType.IsAssignableFrom(p));

            IEnumerable<IMigration> migrations = migrationClasses.Select(t => Activator.CreateInstance(t) as IMigration);

            return migrations;
        }

        public override IDbConnection CreateConnection()
        {
            if (MessageStorageDbConfiguration == null)
            {
                throw new PreConditionFailedException($"{nameof(SetConfiguration)} method executed once before {nameof(CreateConnection)}");
            }

            string connectionStr = MessageStorageDbConfiguration.ConnectionStr;
            var dbConnection = new SqlConnection(connectionStr);
            return dbConnection;
        }

        protected override (string, IEnumerable<IDbDataParameter>) PrepareGetJobCountByStatusCommand(JobStatuses jobStatus, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var jobStatusParameter = new SqlParameter("@JobStatus", (int) jobStatus)
                                     {
                                         SourceColumn = "JobStatus"
                                     };
            var sqlParameters = new List<SqlParameter> {jobStatusParameter};

            string commandText = $"SELECT COUNT(Id) FROM [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] WHERE {jobStatusParameter.SourceColumn} = {jobStatusParameter.ParameterName}";

            return (commandText, sqlParameters);
        }

        protected override (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Message message, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@TraceId", (object) message.TraceId ?? DBNull.Value)
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
            string parameterNames = string.Join(",", sqlParameters.Select(p => $"{p.ParameterName}"));
            string commandText = $"INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.MessageTable}] ({columns}) VALUES ({parameterNames}) SELECT SCOPE_IDENTITY()";

            return (commandText, sqlParameters);
        }

        protected override (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Job job, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@JobStatus", job.JobStatus)
                              {
                                  SourceColumn = "JobStatus"
                              });
            sqlParameters.Add(new SqlParameter("@AssignedHandlerName", job.AssignedHandlerName)
                              {
                                  SourceColumn = "AssignedHandlerName"
                              });
            sqlParameters.Add(new SqlParameter("@LastOperationInfo", (object) job.LastOperationInfo ?? DBNull.Value)
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
            string parameterNames = string.Join(",", sqlParameters.Select(p => $"{p.ParameterName}"));
            string commandText = $"INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] ({columns}) VALUES ({parameterNames}) SELECT SCOPE_IDENTITY()";

            return (commandText, sqlParameters);
        }

        protected override (string, IEnumerable<IDbDataParameter>) PrepareUpdateCommand(Job job, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            var sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@JobStatus", job.JobStatus)
                              {
                                  SourceColumn = "JobStatus"
                              });
            sqlParameters.Add(new SqlParameter("@AssignedHandlerName", job.AssignedHandlerName)
                              {
                                  SourceColumn = "AssignedHandlerName"
                              });
            sqlParameters.Add(new SqlParameter("@LastOperationInfo", (object) job.LastOperationInfo ?? DBNull.Value)
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

            return (commandText, new List<IDbDataParameter>(sqlParameters)
                                 {
                                     idParameter
                                 });
        }

        protected override (string, IEnumerable<IDbDataParameter>) SetFirstWaitingJobToInProgressCommand(MessageStorageDbConfiguration messageStorageDbConfiguration)
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
DECLARE @Updated table( [JobId] bigint)

UPDATE [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] SET [{inProgressJobParameter.SourceColumn}] = {inProgressJobParameter.ParameterName}
OUTPUT INSERTED.Id
INTO @Updated
WHERE  Id = 
(
    SELECT TOP 1 Id 
    FROM [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] WITH (UPDLOCK)
    WHERE {waitingJobParameter.SourceColumn} = {waitingJobParameter.ParameterName}
    ORDER  BY Id
)
SELECT j.Id, j.JobStatus, j.AssignedHandlerName, j.LastOperationInfo, j.LastOperationTime, j.MessageId,
m.CreatedOn, m.SerializedPayload, m.TraceId, m.PayloadClassNamespace ,m.PayloadClassName
    FROM  @Updated u
    INNER JOIN [{messageStorageDbConfiguration.Schema}].[{TableNames.JobTable}] j on j.Id = u.JobId
    INNER JOIN [{messageStorageDbConfiguration.Schema}].[{TableNames.MessageTable}] m on j.MessageId = m.Id
";

            return (commandText, new List<IDbDataParameter> {waitingJobParameter, inProgressJobParameter});
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

            if (!(dataRow[idColumnName] is long id))
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

            if (!(dataRow[messageIdColumnName] is long id))
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

            var message = new Message(id, createOn, traceId, payloadClassName, payloadClassNamespace, serializedPayload);

            return message;
        }
    }
}