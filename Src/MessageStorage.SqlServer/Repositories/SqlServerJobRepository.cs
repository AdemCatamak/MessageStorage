using System;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.PayloadSerializers;
using MessageStorage.SqlServer.DbClient;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace MessageStorage.SqlServer.Repositories
{
    internal class SqlServerJobRepository : IJobRepository
    {
        private readonly ISqlServerRepositoryFactory _creator;
        private readonly ISqlServerMessageStorageTransaction? _transaction;

        public SqlServerJobRepository(ISqlServerRepositoryFactory creator, ISqlServerMessageStorageTransaction? transaction = null)
        {
            _creator = creator;
            _transaction = transaction;
        }

        public Task AddAsync(Job job, CancellationToken cancellationToken = default)
        {
            string script =
                $"INSERT INTO [{_creator.RepositoryConfiguration.Schema}].[Jobs] " +
                $"(Id, CreatedOn, MessageId, MessageHandlerTypeName, JobStatus, " +
                $"LastOperationTime, LastOperationInfo, " +
                $"CurrentExecutionAttemptCount, ExecuteLaterThan) " +
                $"VALUES " +
                $"(@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus," +
                $"@LastOperationTime, @LastOperationInfo," +
                $"@CurrentExecutionAttemptCount, @ExecuteLaterThan)";

            var parameters = new
            {
                Id = job.Id,
                CreatedOn = job.CreatedOn,
                MessageId = job.Message.Id,
                MessageHandlerTypeName = job.MessageHandlerTypeName,
                JobStatus = job.JobStatus,
                LastOperationTime = job.LastOperationTime,
                LastOperationInfo = job.LastOperationInfo,
                CurrentExecutionAttemptCount = job.CurrentExecutionAttemptCount,
                ExecuteLaterThan = job.ExecuteLaterThan,
            };

            return ExecuteAsync(script, parameters, cancellationToken);
        }

        public Task UpdateJobStatusAsync(Job job, CancellationToken cancellationToken = default)
        {
            string script = $"Update [{_creator.RepositoryConfiguration.Schema}].[Jobs] Set " +
                            $"JobStatus = @JobStatus, LastOperationTime = @LastOperationTime, LastOperationInfo = @LastOperationInfo, ExecuteLaterThan = @ExecuteLaterThan " +
                            $"Where Id = @Id";
            var parameters = new
            {
                Id = job.Id,
                JobStatus = (int) job.JobStatus,
                LastOperationTime = job.LastOperationTime,
                LastOperationInfo = job.LastOperationInfo,
                ExecuteLaterThan = job.ExecuteLaterThan,
            };
            return ExecuteAsync(script, parameters, cancellationToken);
        }

        public async Task<Job?> SetFirstQueuedJobToInProgressAsync(CancellationToken cancellationToken = default)
        {
            string script = $@"
DECLARE @Updated table( [Id] nvarchar(255))

UPDATE [{_creator.RepositoryConfiguration.Schema}].[Jobs]
    SET     JobStatus = @TargetStatus,
            LastOperationTime = @TargetLastOperationTime,
            LastOperationInfo = @TargetLastOperationInfo, 
            CurrentExecutionAttemptCount = CurrentExecutionAttemptCount + 1
OUTPUT INSERTED.Id
INTO @Updated
WHERE  Id = 
(
    SELECT TOP 1 j.Id 
    FROM [{_creator.RepositoryConfiguration.Schema}].[Jobs] j WITH (UPDLOCK, READPAST)
    INNER JOIN [{_creator.RepositoryConfiguration.Schema}].[Messages] m on j.MessageId = m.Id
    WHERE JobStatus = @SourceJobStatus
    ORDER BY j.Id
)

SELECT  j.Id, j.MessageHandlerTypeName, j.CreatedOn as JobCreatedOn, j.JobStatus, 
        j.LastOperationTime, j.LastOperationInfo,
        j.CurrentExecutionAttemptCount, j.ExecuteLaterThan,
        j.MessageId, m.Payload, m.CreatedOn as MessageCreatedOn
    FROM  @Updated u
    INNER JOIN [{_creator.RepositoryConfiguration.Schema}].[Jobs] j on j.Id = u.Id
    INNER JOIN [{_creator.RepositoryConfiguration.Schema}].[Messages] m on j.MessageId = m.Id
";

            using ISqlServerMessageStorageConnection connection = _creator.CreateConnection();
            var jobs = (await connection.QueryAsync(script,
                                                    new
                                                    {
                                                        TargetStatus = (int) JobStatus.InProgress,
                                                        TargetLastOperationTime = DateTime.UtcNow,
                                                        TargetLastOperationInfo = JobStatus.InProgress.ToString(),
                                                        SourceJobStatus = (int) JobStatus.Queued
                                                    },
                                                    cancellationToken))
               .Select(row => new Job(row.Id,
                                      new Message((Guid) row.MessageId, PayloadSerializer.Deserialize(row.Payload), (DateTime) row.MessageCreatedOn),
                                      (string) row.MessageHandlerTypeName,
                                      (JobStatus) row.JobStatus,
                                      (DateTime) row.JobCreatedOn,
                                      (DateTime) row.LastOperationTime,
                                      (string) row.LastOperationInfo,
                                      (int) row.CurrentExecutionAttemptCount,
                                      (DateTime) row.ExecuteLaterThan
                                     )
                      );
            return jobs.FirstOrDefault();
        }

        public async Task SetInQueued(string messageHandlerTypeName, int maxExecutionAttemptCount, TimeSpan deferTime, CancellationToken cancellationToken = default)
        {
            string script = $@"
UPDATE ""{_creator.RepositoryConfiguration.Schema}"".jobs
SET     JobStatus = @TargetStatus,
        LastOperationTime = @TargetOperationTime,
        LastOperationInfo = @TargetOperationInfo,
        ExecuteLaterThan = DATEADD(ss, @TargetDeferTime, LastOperationTime)
WHERE   JobStatus = @SourceStatus 
        AND @MaxExecutionAttemptCount > CurrentExecutionAttemptCount
        AND MessageHandlerTypeName = @SourceHandlerName
        ";
            dynamic parameters = new ExpandoObject();
            parameters.TargetStatus = (int) JobStatus.Queued;
            parameters.TargetOperationTime = DateTime.UtcNow;
            parameters.TargetOperationInfo = JobStatus.Queued.ToString();
            parameters.TargetDeferTime = deferTime.TotalSeconds;
            parameters.SourceStatus = (int) JobStatus.Failed;
            parameters.MaxExecutionAttemptCount = maxExecutionAttemptCount;
            parameters.SourceHandlerName = messageHandlerTypeName;

            using ISqlServerMessageStorageConnection connection = _creator.CreateConnection();
            await connection.ExecuteAsync(script,
                                          parameters,
                                          cancellationToken);
        }

        public async Task SetInQueued(string messageHandlerTypeName, TimeSpan maxExecutionTime, CancellationToken cancellationToken = default)
        {
            string script = $@"
UPDATE ""{_creator.RepositoryConfiguration.Schema}"".jobs
SET     JobStatus = @TargetStatus,
        LastOperationTime = @TargetOperationTime,
        LastOperationInfo = @TargetOperationInfo
WHERE   MessageHandlerTypeName = @SourceHandlerName
        AND JobStatus = @SourceStatus 
        AND LastOperationTime < @SourceLastOperationTime
        ";

            dynamic parameters = new ExpandoObject();
            parameters.TargetStatus = (int) JobStatus.Queued;
            parameters.TargetOperationTime = DateTime.UtcNow;
            parameters.TargetOperationInfo = JobStatus.Queued.ToString();
            parameters.SourceHandlerName = messageHandlerTypeName;
            parameters.SourceStatus = (int) JobStatus.InProgress;
            parameters.SourceLastOperationTime = DateTime.UtcNow.AddSeconds(-maxExecutionTime.TotalSeconds);

            using ISqlServerMessageStorageConnection connection = _creator.CreateConnection();
            await connection.ExecuteAsync(script,
                                          parameters,
                                          cancellationToken);
        }

        public async Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken = default)
        {
            string script = $"SELECT count(id) as JobCount FROM [{_creator.RepositoryConfiguration.Schema}].jobs (nolock) WHERE JobStatus = @jobStatus";

            using ISqlServerMessageStorageConnection connection = _creator.CreateConnection();
            var jobCount
                    = (int) ((await connection.QueryAsync(script,
                                                          new
                                                          {
                                                              jobStatus = (int) jobStatus
                                                          },
                                                          cancellationToken)
                             )
                            .First().JobCount
                            )
                ;
            return jobCount;
        }


        private async Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            if (_transaction != null)
            {
                await _transaction.ExecuteAsync(script, parameters, cancellationToken);
            }
            else
            {
                using ISqlServerMessageStorageConnection connection = _creator.CreateConnection();
                await connection.ExecuteAsync(script, parameters, cancellationToken);
            }
        }
    }
}