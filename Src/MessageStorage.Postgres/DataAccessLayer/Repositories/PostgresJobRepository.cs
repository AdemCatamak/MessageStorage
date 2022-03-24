using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.PayloadSerializers;
using Npgsql;

namespace MessageStorage.Postgres.DataAccessLayer.Repositories;

internal class PostgresJobRepository : IJobRepository
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;
    private readonly NpgsqlConnection _npgsqlConnection;
    private readonly PostgresMessageStorageTransaction? _postgresMessageStorageTransaction;

    private string SchemaPlaceHolder => _repositoryContextConfiguration.Schema == null ? "" : $"\"{_repositoryContextConfiguration.Schema}\".";

    public PostgresJobRepository(PostgresRepositoryContextConfiguration repositoryContextConfiguration, NpgsqlConnection npgsqlConnection, PostgresMessageStorageTransaction? postgresMessageStorageTransaction)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
        _npgsqlConnection = npgsqlConnection;
        _postgresMessageStorageTransaction = postgresMessageStorageTransaction;
    }

    public async Task AddAsync(List<Job> jobs, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("INSERT INTO ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs (id, created_on, message_id, message_handler_type_name, job_status, ");
        scriptBuilder.Append("last_operation_time, last_operation_info, ");
        scriptBuilder.Append("max_retry_count, current_retry_count) ");
        scriptBuilder.Append("VALUES ");
        scriptBuilder.Append("(@id, @created_on, @message_id, @message_handler_type_name, @job_status, ");
        scriptBuilder.Append("@last_operation_time, @last_operation_info, ");
        scriptBuilder.Append("@max_retry_count, @current_retry_count)");
        var script = scriptBuilder.ToString();

        var parameters = jobs.Select(j => new
                                          {
                                              id = j.Id,
                                              created_on = j.CreatedOn,
                                              message_id = j.Message.Id,
                                              message_handler_type_name = j.MessageHandlerTypeName,
                                              job_status = j.JobStatus,
                                              last_operation_time = j.LastOperationTime,
                                              last_operation_info = j.LastOperationInfo,
                                              current_retry_count = j.CurrentRetryCount,
                                              max_retry_count = j.MaxRetryCount
                                          })
                             .ToList();

        var commandDefinition = new CommandDefinition(script, parameters, _postgresMessageStorageTransaction?.NpgsqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);

        if (_postgresMessageStorageTransaction != null)
        {
            foreach (Job job in jobs)
            {
                ((IMessageStorageTransaction)_postgresMessageStorageTransaction).AddJobToBeDispatched(job);
            }
        }
    }

    public async Task UpdateStatusAsync(Job job, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("UPDATE ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs SET job_status = @job_status, last_operation_time = @last_operation_time, last_operation_info = @last_operation_info ");
        scriptBuilder.Append("WHERE id = @id");
        var script = scriptBuilder.ToString();
        var parameters = new
                         {
                             job_status = job.JobStatus,
                             last_operation_time = job.LastOperationTime,
                             last_operation_info = job.LastOperationInfo,
                             id = job.Id
                         };

        var commandDefinition = new CommandDefinition(script, parameters, _postgresMessageStorageTransaction?.NpgsqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    public async Task RescueJobsAsync(DateTime lastOperationTimeBeforeThen, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("UPDATE ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs SET job_status = @job_status, last_operation_time = @last_operation_time, last_operation_info = @last_operation_info ");
        scriptBuilder.Append("WHERE last_operation_time < @last_operation_time_before_then and job_status = @source_job_status");

        var script = scriptBuilder.ToString();
        var parameters = new
                         {
                             job_status = JobStatus.Queued,
                             last_operation_time = DateTime.UtcNow,
                             last_operation_info = JobStatus.Queued.ToString(),
                             last_operation_time_before_then = lastOperationTimeBeforeThen,
                             source_job_status = (int)JobStatus.InProgress
                         };

        var commandDefinition = new CommandDefinition(script, parameters, _postgresMessageStorageTransaction?.NpgsqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    public async Task<List<Job>> SetInProgressAsync(DateTime lastOperationTimeBeforeThen, int fetchCount, CancellationToken cancellationToken)
    {
        var script = $@"
UPDATE {SchemaPlaceHolder}jobs j
SET     job_status = @target_status,
        last_operation_time = @target_last_operation_time,
        last_operation_info = @target_last_operation_info,
        current_retry_count = current_retry_count + 1
FROM {SchemaPlaceHolder}messages as m
WHERE m.id = j.message_id and
      j.id =
       (SELECT jo.id
        FROM   {SchemaPlaceHolder}jobs jo
        INNER JOIN {SchemaPlaceHolder}messages me on me.id = jo.message_id
        WHERE  jo.job_status = @source_job_status and jo.last_operation_time < @last_operation_time_before_then
        ORDER BY jo.id
        LIMIT  {fetchCount}
        FOR UPDATE SKIP LOCKED)
RETURNING   j.id, j.message_handler_type_name, j.created_on as job_created_on, j.job_status, 
            j.last_operation_time, j.last_operation_info, 
            j.current_retry_count, j.max_retry_count, 
            j.message_id, m.payload, m.created_on as message_created_on;
";

        var parameters = new
                         {
                             target_status = (int)JobStatus.InProgress,
                             target_last_operation_time = DateTime.UtcNow,
                             target_last_operation_info = JobStatus.InProgress.ToString(),
                             source_job_status = (int)JobStatus.Queued,
                             last_operation_time_before_then = lastOperationTimeBeforeThen
                         };
        var commandDefinition = new CommandDefinition(script, parameters, _postgresMessageStorageTransaction?.NpgsqlTransaction, cancellationToken: cancellationToken);

        List<dynamic> jobsFromDb;
        if (_postgresMessageStorageTransaction != null)
        {
            jobsFromDb = (await _postgresMessageStorageTransaction.NpgsqlTransaction.Connection.QueryAsync(commandDefinition))
               .ToList();
        }
        else
        {
            jobsFromDb = (await _npgsqlConnection.QueryAsync(commandDefinition))
               .ToList();
        }

        List<Job> jobs = jobsFromDb.Select(row => new Job(row.id,
                                                          new Message((Guid)row.message_id, PayloadSerializer.Deserialize(row.payload), (DateTime)row.message_created_on),
                                                          (string)row.message_handler_type_name,
                                                          (JobStatus)row.job_status,
                                                          (DateTime)row.job_created_on,
                                                          (DateTime)row.last_operation_time,
                                                          (string)row.last_operation_info,
                                                          (int)row.current_retry_count,
                                                          (int)row.max_retry_count))
                                   .ToList();

        return jobs;
    }

    public async Task CleanAsync(DateTime lastOperationTimeBeforeThen, bool removeOnlySucceeded, CancellationToken cancellationToken)
    {
        await CleanSucceededAsync(lastOperationTimeBeforeThen, cancellationToken);
        if (!removeOnlySucceeded)
        {
            await CleanFailedAsync(lastOperationTimeBeforeThen, cancellationToken);
        }
    }


    public async Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken)
    {
        var script = $"SELECT count(id) as job_count FROM {SchemaPlaceHolder}jobs WHERE job_status = @job_status";
        var parameters = new
                         {
                             job_status = (int)jobStatus
                         };
        var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);

        var jobCount = (int)(await _npgsqlConnection.QueryAsync(commandDefinition)).First().job_count;
        return jobCount;
    }

    private async Task ExecuteAsync(CommandDefinition commandDefinition)
    {
        if (_postgresMessageStorageTransaction != null)
        {
            await _postgresMessageStorageTransaction.NpgsqlTransaction.Connection.ExecuteAsync(commandDefinition);
        }
        else
        {
            await _npgsqlConnection.ExecuteAsync(commandDefinition);
        }
    }

    private async Task CleanSucceededAsync(DateTime lastOperationTimeBeforeThen, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("DELETE FROM ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs WHERE last_operation_time < @last_operation_time_before_then AND job_status = @job_status");
        var scriptForSucceeded = scriptBuilder.ToString();
        var parameterForSucceeded = new
                                    {
                                        last_operation_time_before_then = lastOperationTimeBeforeThen,
                                        job_status = JobStatus.Succeeded
                                    };
        var commandDefinition = new CommandDefinition(scriptForSucceeded, parameterForSucceeded, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    private async Task CleanFailedAsync(DateTime lastOperationTimeBeforeThen, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("DELETE FROM ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs WHERE last_operation_time < @last_operation_time_before_then AND job_status = @job_status AND current_retry_count >= max_retry_count");
        var scriptForFailed = scriptBuilder.ToString();
        var parametersForFailed = new
                                  {
                                      last_operation_time_before_then = lastOperationTimeBeforeThen,
                                      job_status = JobStatus.Failed
                                  };
        var commandDefinition = new CommandDefinition(scriptForFailed, parametersForFailed, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }
}