using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageStorageClients;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;
using TestUtility.DbUtils;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.ProcessorTest.JobRescuerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class RescueTest : IDisposable
{
    private readonly TestServerFixture _testServerFixture;
    private readonly IServiceScope _serviceScope;
    private readonly IJobRescuerFor<DefaultMessageStorageClient> _jobRescuer;

    public RescueTest(TestServerFixture testServerFixture)
    {
        _testServerFixture = testServerFixture;
        _serviceScope = _testServerFixture.GetServiceScope();
        _jobRescuer = _serviceScope.ServiceProvider.GetRequiredService<IJobRescuerFor<DefaultMessageStorageClient>>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobIsInProgressForALongTerm_JobStatusShouldBeSetAsQueued()
    {
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     Guid.NewGuid(),
                                     "some-message-handler",
                                     JobStatus.InProgress,
                                     DateTime.UtcNow.AddDays(-3),
                                     "info",
                                     2,
                                     1
                                    );

        await Db.Insert.JobIntoPostgresAsync(job);

        Job? jobFromDbBefore = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);
        Assert.Equal(JobStatus.InProgress, jobFromDbBefore!.JobStatus);

        await _jobRescuer.RescueAsync(CancellationToken.None);

        Job? jobFromDbAfter = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
        Assert.Equal(JobStatus.Queued, jobFromDbAfter!.JobStatus);
        Assert.Equal(JobStatus.Queued.ToString(), jobFromDbAfter!.LastOperationInfo);
        Assert.Equal(3, (jobFromDbAfter!.LastOperationTime - job.LastOperationTime).Days);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobIsInProgressForAShortTerm_JobStatusShouldNotBeChanged()
    {
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     Guid.NewGuid(),
                                     "some-message-handler",
                                     JobStatus.InProgress,
                                     DateTime.UtcNow,
                                     "info",
                                     2,
                                     1);

        await Db.Insert.JobIntoPostgresAsync(job);

        Job? jobFromDbBefore = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);
        Assert.Equal(JobStatus.InProgress, jobFromDbBefore!.JobStatus);

        await _jobRescuer.RescueAsync(CancellationToken.None);

        Job? jobFromDbAfter = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
        Assert.Equal(job.JobStatus, jobFromDbAfter!.JobStatus);
        Assert.Equal(job.LastOperationInfo, jobFromDbAfter!.LastOperationInfo);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}