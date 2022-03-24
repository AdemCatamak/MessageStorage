using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.MessageStorageClients;
using MessageStorage.Processor;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.IntegrationTest.Fixtures.MessageHandlers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.StorageCleanerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class CleanMessageTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IStorageCleanerFor<DefaultMessageStorageClient> _storageCleaner;

    const string INSERT_MESSAGE = "INSERT INTO messages (Id, CreatedOn, PayloadTypeName, Payload) VALUES (@Id, @CreatedOn, @PayloadTypeName, @Payload)";

    private const string INSERT_JOB =
        @"
INSERT INTO jobs (Id, CreatedOn, MessageId, MessageHandlerTypeName, JobStatus, LastOperationTime, LastOperationInfo, MaxRetryCount, CurrentRetryCount)
VALUES (@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus, @LastOperationTime, @LastOperationInfo, @MaxRetryCount, @CurrentRetryCount)
";

    public CleanMessageTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _storageCleaner = _serviceScope.ServiceProvider.GetRequiredService<IStorageCleanerFor<DefaultMessageStorageClient>>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenExpiredMessageWithoutJobDoesExist_CleanAsyncShouldRemoveThat()
    {
        var message = new
                      {
                          Id = Guid.NewGuid(),
                          CreatedOn = DateTime.UtcNow.AddDays(-3),
                          PayloadTypeName = typeof(BasicMessage).FullName,
                          Payload = "some-payload"
                      };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_MESSAGE, message);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? messageFromDbBefore = await Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow, CancellationToken.None);

        dynamic? messageFromDbAfter = await Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.Null(messageFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenNewMessageWithoutJobDoesExist_CleanAsyncShouldNotRemoveThat()
    {
        var message = new
                      {
                          Id = Guid.NewGuid(),
                          CreatedOn = DateTime.UtcNow,
                          PayloadTypeName = typeof(BasicMessage).FullName,
                          Payload = "some-payload"
                      };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_MESSAGE, message);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? messageFromDbBefore = await Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow.AddDays(-1), CancellationToken.None);

        dynamic? messageFromDbAfter = await Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbAfter);
    }


    [Fact(Timeout = 3000)]
    public async Task WhenExpiredMessageWithJobDoesExist_CleanAsyncShouldNotRemoveThat()
    {
        var message = new
                      {
                          Id = Guid.NewGuid(),
                          CreatedOn = DateTime.UtcNow.AddDays(-3),
                          PayloadTypeName = typeof(BasicMessage).FullName,
                          Payload = "some-payload"
                      };
        var job = new
                  {
                      Id = Guid.NewGuid(),
                      CreatedOn = DateTime.UtcNow,
                      MessageId = message.Id,
                      MessageHandlerTypeName = "some-message-handler",
                      JobStatus = JobStatus.InProgress,
                      LastOperationTime = DateTime.UtcNow,
                      LastOperationInfo = "info",
                      MaxRetryCount = 2,
                      CurrentRetryCount = 1
                  };
        using (var connection = new SqlConnection(_fixture.ConnectionStr))
        {
            var insertMessageCommandDefinition = new CommandDefinition(INSERT_MESSAGE, message);
            await connection.ExecuteAsync(insertMessageCommandDefinition);
            var insertJobCommandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(insertJobCommandDefinition);
        }

        dynamic? messageFromDbBefore = await Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow, CancellationToken.None);

        dynamic? messageFromDbAfter = await Fetch.MessageFromSqlServerAsync(message.Id);
        Assert.NotNull(messageFromDbAfter);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}