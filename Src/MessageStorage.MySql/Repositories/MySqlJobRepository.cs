using System;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MySql.DbClient;
using MessageStorage.PayloadSerializers;

namespace MessageStorage.MySql.Repositories
{
    internal class MySqlJobRepository : IJobRepository
    {
        private readonly IMySqlRepositoryFactory _creator;
        private readonly IMySqlMessageStorageTransaction? _transaction;

        public MySqlJobRepository(IMySqlRepositoryFactory creator, IMySqlMessageStorageTransaction? transaction = null)
        {
            _creator = creator;
            _transaction = transaction;
        }

        public Task AddAsync(Job job, CancellationToken cancellationToken = default)
        {
            string script =
                $"insert into \"{_creator.RepositoryConfiguration.Schema}\".\"jobs\" " +
                $"(id, created_on, message_id, message_handler_type_name, job_status, " +
                $"last_operation_time, last_operation_info," +
                $"current_execution_attempt_count, execute_later_than) " +
                $"VALUES " +
                $"(@id, @created_on, @message_id, @message_handler_type_name, @job_status, " +
                $"@last_operation_time, @last_operation_info, " +
                $"@current_execution_attempt_count, @execute_later_than)";
            var parameters = new
            {
                id = job.Id,
                created_on = job.CreatedOn,
                message_id = job.Message.Id,
                message_handler_type_name = job.MessageHandlerTypeName,
                job_status = job.JobStatus,
                last_operation_time = job.LastOperationTime,
                last_operation_info = job.LastOperationInfo,
                current_execution_attempt_count = job.CurrentExecutionAttemptCount,
                execute_later_than = job.ExecuteLaterThan,
            };

            return ExecuteAsync(script, parameters, cancellationToken);
        }

        public Task UpdateJobStatusAsync(Job job, CancellationToken cancellationToken = default)
        {
            string script = $"update \"{_creator.RepositoryConfiguration.Schema}\".jobs set " +
                            $"job_status = @job_status, last_operation_time = @last_operation_time, last_operation_info = @last_operation_info, execute_later_than = @execute_later_than " +
                            $"Where id = @id";
            var parameters = new
            {
                id = job.Id,
                job_status = (int) job.JobStatus,
                last_operation_time = job.LastOperationTime,
                last_operation_info = job.LastOperationInfo,
                execute_later_than = job.ExecuteLaterThan,
            };
            return ExecuteAsync(script, parameters, cancellationToken);
        }

        public async Task<Job?> SetFirstQueuedJobToInProgressAsync(CancellationToken cancellationToken = default)
        {
            string script = $@"
UPDATE ""{_creator.RepositoryConfiguration.Schema}"".jobs j
SET     job_status = @target_status,
        last_operation_time = @target_last_operation_time,
        last_operation_info = @target_last_operation_info,
        current_execution_attempt_count = current_execution_attempt_count + 1
FROM ""{_creator.RepositoryConfiguration.Schema}"".messages as m
WHERE m.id = j.message_id and
      j.id =
       (SELECT jo.id
        FROM   ""{_creator.RepositoryConfiguration.Schema}"".jobs jo
        INNER JOIN ""{_creator.RepositoryConfiguration.Schema}"".messages me on me.id = jo.message_id
        WHERE  jo.job_status = @source_job_status and jo.execute_later_than < @source_execute_later_than
        ORDER BY jo.id
        LIMIT  1
        FOR UPDATE SKIP LOCKED)
RETURNING   j.id, j.message_handler_type_name, j.created_on as job_created_on, j.job_status, 
            j.last_operation_time, j.last_operation_info, 
            j.current_execution_attempt_count, j.execute_later_than,
            j.message_id, m.payload, m.created_on as message_created_on;
";
            using var connection = _creator.CreateConnection();
            var jobs = (await connection.QueryAsync(script,
                                                    new
                                                    {
                                                        target_status = (int) JobStatus.InProgress,
                                                        target_last_operation_time = DateTime.UtcNow,
                                                        target_last_operation_info = JobStatus.InProgress.ToString(),
                                                        source_job_status = (int) JobStatus.Queued,
                                                        source_execute_later_than = DateTime.UtcNow
                                                    },
                                                    cancellationToken))
               .Select(row => new Job(row.id,
                                      new Message((Guid) row.message_id, PayloadSerializer.Deserialize(row.payload), (DateTime) row.message_created_on),
                                      (string) row.message_handler_type_name,
                                      (JobStatus) row.job_status,
                                      (DateTime) row.job_created_on,
                                      (DateTime) row.last_operation_time,
                                      (string) row.last_operation_info,
                                      (int) row.current_execution_attempt_count,
                                      (DateTime) row.execute_later_than
                                     )
                      );
            return jobs.FirstOrDefault();
        }

        public async Task SetInQueued(string messageHandlerTypeName, int maxExecutionAttemptCount, TimeSpan deferTime, CancellationToken cancellationToken = default)
        {
            string script = $@"
UPDATE ""{_creator.RepositoryConfiguration.Schema}"".jobs
SET     job_status = @target_job_status,
        last_operation_time = @target_last_operation_time,
        last_operation_info = @target_last_operation_info,
        execute_later_than = (last_operation_time + @target_defer_time * INTERVAL '1 second')
WHERE   job_status = @source_job_status
        AND @max_execution_attempt_count > current_execution_attempt_count
        AND message_handler_type_name = @source_handler_type
        ";
            dynamic parameters = new ExpandoObject();
            parameters.target_job_status = (int) JobStatus.Queued;
            parameters.target_last_operation_time = DateTime.UtcNow;
            parameters.target_last_operation_info = JobStatus.Queued.ToString();
            parameters.target_defer_time = deferTime.TotalSeconds;
            parameters.source_job_status = (int) JobStatus.Failed;
            parameters.max_execution_attempt_count = maxExecutionAttemptCount;
            parameters.source_handler_type = messageHandlerTypeName;

            using IMySqlMessageStorageConnection connection = _creator.CreateConnection();
            await connection.ExecuteAsync(script,
                                          parameters,
                                          cancellationToken);
        }

        public async Task SetInQueued(string messageHandlerTypeName, TimeSpan maxExecutionTime, CancellationToken cancellationToken = default)
        {
            string script = $@"
UPDATE ""{_creator.RepositoryConfiguration.Schema}"".jobs
SET     job_status = @target_job_status,
        last_operation_time = @target_last_operation_time,
        last_operation_info = @target_last_operation_info
WHERE   message_handler_type_name = @source_handler_type
        AND job_status = @source_job_status
        AND last_operation_time < @source_last_operation_time
        ";
            dynamic parameters = new ExpandoObject();
            parameters.target_job_status = (int) JobStatus.Queued;
            parameters.target_last_operation_time = DateTime.UtcNow;
            parameters.target_last_operation_info = JobStatus.Queued.ToString();
            parameters.source_handler_type = messageHandlerTypeName;
            parameters.source_job_status = (int) JobStatus.InProgress;
            parameters.source_last_operation_time = DateTime.UtcNow.AddSeconds(-maxExecutionTime.TotalSeconds);

            using IMySqlMessageStorageConnection connection = _creator.CreateConnection();
            await connection.ExecuteAsync(script,
                                          parameters,
                                          cancellationToken);
        }

        public async Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken = default)
        {
            string script = $"SELECT count(id) as job_count FROM \"{_creator.RepositoryConfiguration.Schema}\".jobs WHERE job_status = @job_status";

            using IMySqlMessageStorageConnection connection = _creator.CreateConnection();
            var jobCount
                    = (int) ((await connection.QueryAsync(script,
                                                          new
                                                          {
                                                              job_status = (int) jobStatus
                                                          },
                                                          cancellationToken)
                             )
                            .First().job_count
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
                using var connection = _creator.CreateConnection();
                await connection.ExecuteAsync(script, parameters, cancellationToken);
            }
        }
    }
}