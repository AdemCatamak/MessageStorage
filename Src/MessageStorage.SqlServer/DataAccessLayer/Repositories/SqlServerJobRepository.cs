using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.PayloadSerializers;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.DataAccessLayer.Repositories;

public class SqlServerJobRepository : IJobRepository
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;
    private readonly SqlConnection _sqlConnection;
    private readonly SqlServerMessageStorageTransaction? _sqlServerMessageStorageTransaction;

    private string SchemaPlaceHolder => _repositoryContextConfiguration.Schema == null ? "" : $"[{_repositoryContextConfiguration.Schema}].";

    public SqlServerJobRepository(SqlServerRepositoryContextConfiguration repositoryContextConfiguration, SqlConnection sqlConnection, SqlServerMessageStorageTransaction? sqlServerMessageStorageTransaction)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
        _sqlConnection = sqlConnection;
        _sqlServerMessageStorageTransaction = sqlServerMessageStorageTransaction;
    }

    public async Task AddAsync(List<Job> jobs, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("INSERT INTO ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("[Jobs] (Id, CreatedOn, MessageId, MessageHandlerTypeName, JobStatus, ");
        scriptBuilder.Append("LastOperationTime, LastOperationInfo, ");
        scriptBuilder.Append("MaxRetryCount, CurrentRetryCount) ");
        scriptBuilder.Append("VALUES ");
        scriptBuilder.Append("(@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus, @LastOperationTime, @LastOperationInfo, @MaxRetryCount, @CurrentRetryCount)");
        var script = scriptBuilder.ToString();

        var parameters = jobs.Select(j => new
                                          {
                                              Id = j.Id,
                                              CreatedOn = j.CreatedOn,
                                              MessageId = j.Message.Id,
                                              MessageHandlerTypeName = j.MessageHandlerTypeName,
                                              JobStatus = j.JobStatus,
                                              LastOperationTime = j.LastOperationTime,
                                              LastOperationInfo = j.LastOperationInfo,
                                              MaxRetryCount = j.MaxRetryCount,
                                              CurrentRetryCount = j.CurrentRetryCount,
                                          })
                             .ToList();

        var commandDefinition = new CommandDefinition(script, parameters, _sqlServerMessageStorageTransaction?.SqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);

        if (_sqlServerMessageStorageTransaction != null)
        {
            foreach (Job job in jobs)
            {
                ((IMessageStorageTransaction)_sqlServerMessageStorageTransaction).AddJobToBeDispatched(job);
            }
        }
    }

    public async Task UpdateStatusAsync(Job job, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("UPDATE ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("[Jobs] SET JobStatus = @JobStatus, LastOperationTime = @LastOperationTime, LastOperationInfo = @LastOperationInfo ");
        scriptBuilder.Append("WHERE Id = @Id");
        var script = scriptBuilder.ToString();
        var parameters = new
                         {
                             JobStatus = job.JobStatus,
                             LastOperationTime = job.LastOperationTime,
                             LastOperationInfo = job.LastOperationInfo,
                             Id = job.Id
                         };

        var commandDefinition = new CommandDefinition(script, parameters, _sqlServerMessageStorageTransaction?.SqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    public async Task RescueJobsAsync(DateTime lastOperationTimeBeforeThen, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("UPDATE ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("[Jobs] SET JobStatus = @JobStatus, LastOperationTime = @LastOperationTime, LastOperationInfo = @LastOperationInfo ");
        scriptBuilder.Append("WHERE LastOperationTime < @LastOperationTimeBeforeThen AND JobStatus = @SourceJobStatus");
        var script = scriptBuilder.ToString();
        var parameters = new
                         {
                             JobStatus = JobStatus.Queued,
                             LastOperationTime = DateTime.UtcNow,
                             LastOperationInfo = JobStatus.Queued.ToString(),
                             LastOperationTimeBeforeThen = lastOperationTimeBeforeThen,
                             SourceJobStatus = JobStatus.InProgress
                         };

        var commandDefinition = new CommandDefinition(script, parameters, _sqlServerMessageStorageTransaction?.SqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    public async Task<List<Job>> SetInProgressAsync(DateTime lastOperationTimeBeforeThen, int fetchCount, CancellationToken cancellationToken)
    {
        var script = $@"
DECLARE @Updated table( [Id] nvarchar(255))

UPDATE {SchemaPlaceHolder}[Jobs]
    SET     JobStatus = @TargetStatus,
            LastOperationTime = @TargetLastOperationTime,
            LastOperationInfo = @TargetLastOperationInfo, 
            CurrentRetryCount = CurrentRetryCount + 1
OUTPUT INSERTED.Id
INTO @Updated
WHERE  Id = 
(
    SELECT TOP 1 j.Id 
    FROM {SchemaPlaceHolder}[Jobs] j WITH (UPDLOCK, READPAST)
    INNER JOIN {SchemaPlaceHolder}[Messages] m on j.MessageId = m.Id
    WHERE JobStatus = @SourceJobStatus and LastOperationTime < @LastOperationTimeBeforeThen
    ORDER BY j.Id
)

SELECT  j.Id, j.MessageHandlerTypeName, j.CreatedOn as JobCreatedOn, j.JobStatus, 
        j.LastOperationTime, j.LastOperationInfo,
        j.CurrentRetryCount, j.MaxRetryCount, 
        j.MessageId, m.Payload, m.CreatedOn as MessageCreatedOn
    FROM  @Updated u
    INNER JOIN {SchemaPlaceHolder}[Jobs] j on j.Id = u.Id
    INNER JOIN {SchemaPlaceHolder}[Messages] m on j.MessageId = m.Id
";
        var parameters = new
                         {
                             TargetStatus = JobStatus.InProgress,
                             TargetLastOperationTime = DateTime.UtcNow,
                             TargetLastOperationInfo = JobStatus.InProgress.ToString(),
                             SourceJobStatus = JobStatus.Queued,
                             LastOperationTimeBeforeThen = lastOperationTimeBeforeThen
                         };

        var commandDefinition = new CommandDefinition(script, parameters, _sqlServerMessageStorageTransaction?.SqlTransaction, cancellationToken: cancellationToken);

        List<dynamic> jobsFromDb;
        if (_sqlServerMessageStorageTransaction != null)
        {
            jobsFromDb = (await _sqlServerMessageStorageTransaction.SqlTransaction.Connection.QueryAsync(commandDefinition))
               .ToList();
        }
        else
        {
            jobsFromDb = (await _sqlConnection.QueryAsync(commandDefinition))
               .ToList();
        }

        List<Job> jobList = jobsFromDb.Select(row => new Job(row.Id,
                                                             new Message((Guid)row.MessageId, PayloadSerializer.Deserialize(row.Payload), (DateTime)row.MessageCreatedOn),
                                                             (string)row.MessageHandlerTypeName,
                                                             (JobStatus)row.JobStatus,
                                                             (DateTime)row.JobCreatedOn,
                                                             (DateTime)row.LastOperationTime,
                                                             (string)row.LastOperationInfo,
                                                             (int)row.CurrentRetryCount,
                                                             (int)row.MaxRetryCount))
                                      .ToList();
        return jobList;
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
        var script = $"SELECT count(id) as JobCount FROM {SchemaPlaceHolder}Jobs (nolock) WHERE JobStatus = @jobStatus";
        var parameters = new
                         {
                             jobStatus = (int)jobStatus
                         };
        var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
        var jobCount = (int)(await _sqlConnection.QueryAsync(commandDefinition)).First().JobCount;

        return jobCount;
    }

    private async Task ExecuteAsync(CommandDefinition commandDefinition)
    {
        if (_sqlServerMessageStorageTransaction != null)
        {
            await _sqlServerMessageStorageTransaction.SqlTransaction.Connection.ExecuteAsync(commandDefinition);
        }
        else
        {
            await _sqlConnection.ExecuteAsync(commandDefinition);
        }
    }

    private async Task CleanSucceededAsync(DateTime lastOperationTimeBeforeThen, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("DELETE FROM ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs WHERE LastOperationTime < @LastOperationTimeBeforeThen AND JobStatus = @JobStatus");
        var scriptForSucceeded = scriptBuilder.ToString();
        var parameterForSucceeded = new
                                    {
                                        LastOperationTimeBeforeThen = lastOperationTimeBeforeThen,
                                        JobStatus = JobStatus.Succeeded
                                    };
        var commandDefinition = new CommandDefinition(scriptForSucceeded, parameterForSucceeded, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    private async Task CleanFailedAsync(DateTime lastOperationTimeBeforeThen, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder("DELETE FROM ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs WHERE LastOperationTime < @LastOperationTimeBeforeThen AND JobStatus = @JobStatus AND CurrentRetryCount >= MaxRetryCount");
        var scriptForFailed = scriptBuilder.ToString();
        var parametersForFailed = new
                                  {
                                      LastOperationTimeBeforeThen = lastOperationTimeBeforeThen,
                                      JobStatus = JobStatus.Failed
                                  };
        var commandDefinition = new CommandDefinition(scriptForFailed, parametersForFailed, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }
}