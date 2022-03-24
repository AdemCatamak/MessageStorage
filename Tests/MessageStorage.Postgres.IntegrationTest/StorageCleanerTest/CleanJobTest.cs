using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.MessageStorageClients;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.StorageCleanerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class CleanJobTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IStorageCleanerFor<DefaultMessageStorageClient> _storageCleaner;

    private const string INSERT_JOB =
        @"
INSERT INTO jobs (id, created_on, message_id, message_handler_type_name, job_status, last_operation_time, last_operation_info, max_retry_count, current_retry_count)
VALUES (@id, @created_on, @message_id, @message_handler_type_name, @job_status, @last_operation_time, @last_operation_info,@max_retry_count, @current_retry_count)
";

    public CleanJobTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _storageCleaner = _serviceScope.ServiceProvider.GetRequiredService<IStorageCleanerFor<DefaultMessageStorageClient>>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobSucceededAndExpired_CleanAsyncShouldRemoveThat()
    {
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = Guid.NewGuid(),
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.Succeeded,
                      last_operation_time = DateTime.UtcNow.AddDays(-3),
                      last_operation_info = "info",
                      max_retry_count = 2,
                      current_retry_count = 1
                  };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, true, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromPostgresAsync(job.id);
        Assert.Null(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobSucceededAndNew_CleanAsyncShouldNotRemoveThat()
    {
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = Guid.NewGuid(),
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.Succeeded,
                      last_operation_time = DateTime.UtcNow,
                      last_operation_info = "info",
                      max_retry_count = 2,
                      current_retry_count = 1
                  };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow.AddDays(-1), true, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceeded_CleanAsyncShouldNotRemoveThat()
    {
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = Guid.NewGuid(),
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.Failed,
                      last_operation_time = DateTime.UtcNow.AddDays(-3),
                      last_operation_info = "info",
                      max_retry_count = 2,
                      current_retry_count = 2
                  };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, true, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceededSetFalseAndMaxRetryLimitExceeded_CleanAsyncShouldRemoveThat()
    {
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = Guid.NewGuid(),
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.Failed,
                      last_operation_time = DateTime.UtcNow.AddDays(-3),
                      last_operation_info = "info",
                      max_retry_count = 2,
                      current_retry_count = 2
                  };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, false, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromPostgresAsync(job.id);
        Assert.Null(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceededSetFalseAndMaxRetryLimitNotExceeded_CleanAsyncShouldNotRemoveThat()
    {
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = Guid.NewGuid(),
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.Failed,
                      last_operation_time = DateTime.UtcNow.AddDays(-3),
                      last_operation_info = "info",
                      max_retry_count = 3,
                      current_retry_count = 2
                  };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, false, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbAfter);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}