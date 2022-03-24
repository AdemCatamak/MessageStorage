using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.MessageStorageClients;
using MessageStorage.Processor;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.StorageCleanerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class CleanJobTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IStorageCleanerFor<DefaultMessageStorageClient> _storageCleaner;

    private const string INSERT_JOB =
        @"
INSERT INTO jobs (Id, CreatedOn, MessageId, MessageHandlerTypeName, JobStatus, LastOperationTime, LastOperationInfo, MaxRetryCount, CurrentRetryCount)
VALUES (@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus, @LastOperationTime, @LastOperationInfo, @MaxRetryCount, @CurrentRetryCount)
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
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = Guid.NewGuid(),
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.Succeeded,
                      LastOperationTime = DateTime.UtcNow.AddDays(-3),
                      LastOperationInfo = "info",
                      MaxRetryCount = 2,
                      CurrentRetryCount = 1
                  };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, true, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.Null(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobSucceededAndNew_CleanAsyncShouldNotRemoveThat()
    {
        var job = new
                  {
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = Guid.NewGuid(),
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.Succeeded,
                      LastOperationTime = DateTime.UtcNow,
                      LastOperationInfo = "info",
                      MaxRetryCount = 2,
                      CurrentRetryCount = 1
                  };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow.AddDays(-1), true, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceeded_CleanAsyncShouldNotRemoveThat()
    {
        var job = new
                  {
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = Guid.NewGuid(),
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.Failed,
                      LastOperationTime = DateTime.UtcNow.AddDays(-3),
                      LastOperationInfo = "info",
                      MaxRetryCount = 2,
                      CurrentRetryCount = 1
                  };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, true, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceededSetFalseAndMaxRetryLimitExceeded_CleanAsyncShouldRemoveThat()
    {
        var job = new
                  {
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = Guid.NewGuid(),
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.Failed,
                      LastOperationTime = DateTime.UtcNow.AddDays(-3),
                      LastOperationInfo = "info",
                      MaxRetryCount = 2,
                      CurrentRetryCount = 2
                  };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, false, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.Null(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceededSetFalseAndMaxRetryLimitNotExceeded_CleanAsyncShouldNotRemoveThat()
    {
        var job = new
                  {
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = Guid.NewGuid(),
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.Failed,
                      LastOperationTime = DateTime.UtcNow.AddDays(-3),
                      LastOperationInfo = "info",
                      MaxRetryCount = 3,
                      CurrentRetryCount = 2
                  };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, false, CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}