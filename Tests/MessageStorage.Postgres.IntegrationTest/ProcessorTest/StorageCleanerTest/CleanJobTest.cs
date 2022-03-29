using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageStorageClients;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;
using TestUtility.DbUtils;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.ProcessorTest.StorageCleanerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class CleanJobTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IStorageCleanerFor<DefaultMessageStorageClient> _storageCleaner;

    public CleanJobTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _storageCleaner = _serviceScope.ServiceProvider.GetRequiredService<IStorageCleanerFor<DefaultMessageStorageClient>>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobSucceededAndExpired_CleanAsyncShouldRemoveThat()
    {
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     Guid.NewGuid(),
                                     "some-message-handler",
                                     JobStatus.Succeeded,
                                     DateTime.UtcNow.AddDays(-3),
                                     "info",
                                     2,
                                     1
                                    );

        await Db.Insert.JobIntoPostgresAsync(job);

        Job? jobFromDbBefore = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, true, CancellationToken.None);

        Job? jobFromDbAfter = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.Null(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobSucceededAndNew_CleanAsyncShouldNotRemoveThat()
    {
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     Guid.NewGuid(),
                                     "some-message-handler",
                                     JobStatus.Succeeded,
                                     DateTime.UtcNow,
                                     "info",
                                     2,
                                     1
                                    );
        await Db.Insert.JobIntoPostgresAsync(job);

        Job? jobFromDbBefore = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow.AddDays(-1), true, CancellationToken.None);

        Job? jobFromDbAfter = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceeded_CleanAsyncShouldNotRemoveThat()
    {
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     Guid.NewGuid(),
                                     "some-message-handler",
                                     JobStatus.Failed,
                                     DateTime.UtcNow.AddDays(-3),
                                     "info",
                                     2,
                                     2
                                    );

        await Db.Insert.JobIntoPostgresAsync(job);

        Job? jobFromDbBefore = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, true, CancellationToken.None);

        Job? jobFromDbAfter = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceededSetFalseAndMaxRetryLimitExceeded_CleanAsyncShouldRemoveThat()
    {
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     Guid.NewGuid(),
                                     "some-message-handler",
                                     JobStatus.Failed,
                                     DateTime.UtcNow.AddDays(-3),
                                     "info",
                                     2,
                                     2
                                    );

        await Db.Insert.JobIntoPostgresAsync(job);

        Job? jobFromDbBefore = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, false, CancellationToken.None);

        Job? jobFromDbAfter = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.Null(jobFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenJobFailedAndCleanOnlySucceededSetFalseAndMaxRetryLimitNotExceeded_CleanAsyncShouldNotRemoveThat()
    {
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     Guid.NewGuid(),
                                     "some-message-handler",
                                     JobStatus.Failed,
                                     DateTime.UtcNow.AddDays(-3),
                                     "info",
                                     3,
                                     2
                                    );

        await Db.Insert.JobIntoPostgresAsync(job);

        Job? jobFromDbBefore = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbBefore);

        await _storageCleaner.CleanJobsAsync(DateTime.UtcNow, false, CancellationToken.None);

        Job? jobFromDbAfter = await Db.Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDbAfter);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}