using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.MessageStorageClients;
using MessageStorage.Processor;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.JobRescuerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class RescueTest : IDisposable
{
    private readonly TestServerFixture _testServerFixture;
    private readonly IServiceScope _serviceScope;
    private readonly IJobRescuerFor<DefaultMessageStorageClient> _jobRescuer;

    private const string INSERT_JOB =
        @"
INSERT INTO jobs (Id, CreatedOn, MessageId, MessageHandlerTypeName, JobStatus, LastOperationTime, LastOperationInfo, MaxRetryCount, CurrentRetryCount)
VALUES (@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus, @LastOperationTime, @LastOperationInfo, @MaxRetryCount, @CurrentRetryCount)
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
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = Guid.NewGuid(),
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.InProgress,
                      LastOperationTime = DateTime.UtcNow.AddDays(-3),
                      LastOperationInfo = "info",
                      MaxRetryCount = 2,
                      CurrentRetryCount = 1
                  };

        using (var connection = new SqlConnection(_testServerFixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);
        Assert.Equal((int)JobStatus.InProgress, jobFromDbBefore!.JobStatus);

        await _jobRescuer.RescueAsync(CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
        Assert.Equal((int)JobStatus.Queued, jobFromDbAfter!.JobStatus);
        Assert.Equal(JobStatus.Queued.ToString(), jobFromDbAfter.LastOperationInfo);
        Assert.Equal(3, ((DateTime)jobFromDbAfter.LastOperationTime - job.LastOperationTime).Days);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobIsInProgressForAShortTerm_JobStatusShouldNotBeChanged()
    {
        var job = new
                  {
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = Guid.NewGuid(),
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.InProgress,
                      LastOperationTime = DateTime.UtcNow,
                      LastOperationInfo = "info",
                      MaxRetryCount = 2,
                      CurrentRetryCount = 1
                  };

        using (var connection = new SqlConnection(_testServerFixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? jobFromDbBefore = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);
        Assert.Equal((int)JobStatus.InProgress, jobFromDbBefore!.JobStatus);

        await _jobRescuer.RescueAsync(CancellationToken.None);

        dynamic? jobFromDbAfter = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
        Assert.Equal((int)job.JobStatus, jobFromDbAfter!.JobStatus);
        Assert.Equal(job.LastOperationInfo, jobFromDbAfter.LastOperationInfo);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}