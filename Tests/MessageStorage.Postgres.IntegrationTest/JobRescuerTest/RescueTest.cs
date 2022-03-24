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

namespace MessageStorage.Postgres.IntegrationTest.JobRescuerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class RescueTest : IDisposable
{
    private readonly TestServerFixture _testServerFixture;
    private readonly IServiceScope _serviceScope;
    private readonly IJobRescuerFor<DefaultMessageStorageClient> _jobRescuer;

    private const string INSERT_JOB =
        @"
INSERT INTO jobs (id, created_on, message_id, message_handler_type_name, job_status, last_operation_time, last_operation_info, max_retry_count, current_retry_count)
VALUES (@id, @created_on, @message_id, @message_handler_type_name, @job_status, @last_operation_time, @last_operation_info,@max_retry_count, @current_retry_count)
";

    public RescueTest(TestServerFixture testServerFixture)
    {
        _testServerFixture = testServerFixture;
        _serviceScope = _testServerFixture.GetServiceScope();
        _jobRescuer = _serviceScope.ServiceProvider.GetRequiredService<IJobRescuerFor<DefaultMessageStorageClient>>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobIsInProgressForALongTerm_JobStatusShouldBeSetAsQueued()
    {
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = Guid.NewGuid(),
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.InProgress,
                      last_operation_time = DateTime.UtcNow.AddDays(-3),
                      last_operation_info = "info",
                      max_retry_count = 2,
                      current_retry_count = 1
                  };

        using (var connection = new NpgsqlConnection(_testServerFixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbBefore);
        Assert.Equal((int)JobStatus.InProgress, jobFromDbBefore!.job_status);

        await _jobRescuer.RescueAsync(CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbAfter);
        Assert.Equal((int)JobStatus.Queued, jobFromDbAfter!.job_status);
        Assert.Equal(JobStatus.Queued.ToString(), jobFromDbAfter!.last_operation_info);
        Assert.Equal(3, ((DateTime)jobFromDbAfter!.last_operation_time - job.last_operation_time).Days);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobIsInProgressForAShortTerm_JobStatusShouldNotBeChanged()
    {
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = Guid.NewGuid(),
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.InProgress,
                      last_operation_time = DateTime.UtcNow,
                      last_operation_info = "info",
                      max_retry_count = 2,
                      current_retry_count = 1
                  };

        using (var connection = new NpgsqlConnection(_testServerFixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbBefore);
        Assert.Equal((int)JobStatus.InProgress, jobFromDbBefore!.job_status);

        await _jobRescuer.RescueAsync(CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromPostgresAsync(job.id);
        Assert.NotNull(jobFromDbAfter);
        Assert.Equal((int)job.job_status, jobFromDbAfter!.job_status);
        Assert.Equal(job.last_operation_info, jobFromDbAfter!.last_operation_info);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}