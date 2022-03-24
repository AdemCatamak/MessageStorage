using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.MessageStorageClients;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Postgres.IntegrationTest.Fixtures.MessageHandlers;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.StorageCleanerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class CleanMessageTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IStorageCleanerFor<DefaultMessageStorageClient> _storageCleaner;

    const string INSERT_MESSAGE = "INSERT INTO messages (id, created_on, payload_type_name, payload) VALUES (@id, @created_on, @payload_type_name, @payload)";

    private const string INSERT_JOB =
        @"
INSERT INTO jobs (id, created_on, message_id, message_handler_type_name, job_status, last_operation_time, last_operation_info, max_retry_count, current_retry_count)
VALUES
(@id, @created_on, @message_id, @message_handler_type_name, @job_status, @last_operation_time, @last_operation_info,@max_retry_count, @current_retry_count)
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
                          id = Guid.NewGuid(),
                          created_on = DateTime.UtcNow.AddDays(-3),
                          payload_type_name = typeof(BasicMessage).FullName,
                          payload = "some-payload"
                      };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_MESSAGE, message);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? messageFromDbBefore = await Fetch.MessageFromPostgresAsync(message.id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow, CancellationToken.None);

        dynamic? messageFromDbAfter = await Fetch.MessageFromPostgresAsync(message.id);
        Assert.Null(messageFromDbAfter);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenNewMessageWithoutJobDoesExist_CleanAsyncShouldNotRemoveThat()
    {
        var message = new
                      {
                          id = Guid.NewGuid(),
                          created_on = DateTime.UtcNow,
                          payload_type_name = typeof(BasicMessage).FullName,
                          payload = "some-payload"
                      };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var commandDefinition = new CommandDefinition(INSERT_MESSAGE, message);
            await connection.ExecuteAsync(commandDefinition);
        }

        dynamic? messageFromDbBefore = await Fetch.MessageFromPostgresAsync(message.id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow.AddDays(-1), CancellationToken.None);

        dynamic? messageFromDbAfter = await Fetch.MessageFromPostgresAsync(message.id);
        Assert.NotNull(messageFromDbAfter);
    }


    [Fact(Timeout = 3000)]
    public async Task WhenExpiredMessageWithJobDoesExist_CleanAsyncShouldNotRemoveThat()
    {
        var message = new
                      {
                          id = Guid.NewGuid(),
                          created_on = DateTime.UtcNow.AddDays(-3),
                          payload_type_name = typeof(BasicMessage).FullName,
                          payload = "some-payload"
                      };
        var job = new
                  {
                      id = Guid.NewGuid(),
                      created_on = DateTime.UtcNow,
                      message_id = message.id,
                      message_handler_type_name = "some-message-handler",
                      job_status = JobStatus.InProgress,
                      last_operation_time = DateTime.UtcNow,
                      last_operation_info = "info",
                      max_retry_count = 2,
                      current_retry_count = 1
                  };
        using (var connection = new NpgsqlConnection(_fixture.ConnectionStr))
        {
            var insertMessageCommandDefinition = new CommandDefinition(INSERT_MESSAGE, message);
            await connection.ExecuteAsync(insertMessageCommandDefinition);
            var insertJobCommandDefinition = new CommandDefinition(INSERT_JOB, job);
            await connection.ExecuteAsync(insertJobCommandDefinition);
        }

        dynamic? messageFromDbBefore = await Fetch.MessageFromPostgresAsync(message.id);
        Assert.NotNull(messageFromDbBefore);

        await _storageCleaner.CleanMessageAsync(DateTime.UtcNow, CancellationToken.None);

        dynamic? messageFromDbAfter = await Fetch.MessageFromPostgresAsync(message.id);
        Assert.NotNull(messageFromDbAfter);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}