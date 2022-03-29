using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageStorageClients;
using MessageStorage.Processor;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.IntegrationTest.Fixtures.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using TestUtility.DbUtils;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.ProcessorTest.StorageCleanerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class CleanMessageTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IStorageCleanerFor<DefaultMessageStorageClient> _storageCleaner;

    public CleanMessageTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _storageCleaner = _serviceScope.ServiceProvider.GetRequiredService<IStorageCleanerFor<DefaultMessageStorageClient>>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenExpiredMessageWithoutJobDoesExist_CleanAsyncShouldRemoveThat()
    {
        var message = new IInsert.MessageDto(Guid.NewGuid(),
                                             DateTime.UtcNow.AddDays(-3),
                                             typeof(BasicMessage).FullName!,
                                             "some-payload");
        await Db.Insert.MessageIntoSqlServerAsync(message);

        Message? messageFromDbBefore = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow, CancellationToken.None);

        Message? messageFromDbAfter = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.Null(messageFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenNewMessageWithoutJobDoesExist_CleanAsyncShouldNotRemoveThat()
    {
        var message = new IInsert.MessageDto(
                                             Guid.NewGuid(),
                                             DateTime.UtcNow,
                                             typeof(BasicMessage).FullName!,
                                             "some-payload");

        await Db.Insert.MessageIntoSqlServerAsync(message);

        Message? messageFromDbBefore = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow.AddDays(-1), CancellationToken.None);

        Message? messageFromDbAfter = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbAfter);
    }


    [Fact(Timeout = 3000)]
    public async Task WhenExpiredMessageWithJobDoesExist_CleanAsyncShouldNotRemoveThat()
    {
        var message = new IInsert.MessageDto(Guid.NewGuid(),
                                             DateTime.UtcNow.AddDays(-3),
                                             typeof(BasicMessage).FullName!,
                                             "some-payload"
                                            );
        var job = new IInsert.JobDto(Guid.NewGuid(),
                                     DateTime.UtcNow,
                                     message.Id,
                                     "some-message-handler",
                                     JobStatus.InProgress,
                                     DateTime.UtcNow,
                                     "info",
                                     2,
                                     1
                                    );

        await Db.Insert.MessageIntoSqlServerAsync(message);
        await Db.Insert.JobIntoSqlServerAsync(job);

        Message? messageFromDbBefore = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow, CancellationToken.None);

        Message? messageFromDbAfter = await Db.Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbAfter);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}