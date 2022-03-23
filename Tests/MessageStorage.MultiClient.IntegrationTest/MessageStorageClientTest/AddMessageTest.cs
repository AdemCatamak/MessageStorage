using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageStorage.MultiClient.IntegrationTest.Fixtures;
using MessageStorage.MultiClient.IntegrationTest.Fixtures.MessageHandlers;
using MessageStorage.MultiClient.IntegrationTest.Fixtures.SecondaryMessageStorageSection;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.MultiClient.IntegrationTest.MessageStorageClientTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class AddMessageTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IMessageStorageClient _messageStorageClient;
    private readonly ISecondaryMessageStorageClient _secondaryMessageStorageClient;

    public AddMessageTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _messageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
        _secondaryMessageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<ISecondaryMessageStorageClient>();
    }


    [Fact(Timeout = 3000)]
    public async Task WhenThereIsCompatibleMessageHandler_JobListShouldNotBeEmpty()
    {
        Message sqlServerMessage;
        Message postgresMessage;
        var sqlServerBasicMessage = new BasicMessage("some-message");
        var postgresBasicMessage = new BasicMessage("some-message");

        (sqlServerMessage, List<Job> sqlServerJobs) = await _messageStorageClient.AddMessageAsync(sqlServerBasicMessage);
        (postgresMessage, List<Job> postgresJobs) = await _secondaryMessageStorageClient.AddMessageAsync(postgresBasicMessage);

        Assert.Single(sqlServerJobs);
        Assert.Single(postgresJobs);
        Job sqlServerJob = sqlServerJobs.First();
        Job postgresJob = postgresJobs.First();

        dynamic? messageFromSqlServer = await Fetch.MessageFromSqlServerAsync(sqlServerMessage.Id);
        Assert.NotNull(messageFromSqlServer);
        Assert.Equal(sqlServerMessage.Id, messageFromSqlServer!.Id);

        dynamic? jobFromSqlServer = await Fetch.JobFromSqlServerAsync(sqlServerJob.Id);
        Assert.NotNull(jobFromSqlServer);
        Assert.Equal(sqlServerJob.Id, jobFromSqlServer!.Id);

        dynamic? messageFromPostgres = await Fetch.MessageFromPostgresAsync(postgresMessage.Id);
        Assert.NotNull(messageFromPostgres);
        Assert.Equal(postgresMessage.Id, messageFromPostgres!.id);

        dynamic? jobFromPostgres = await Fetch.JobFromPostgresAsync(postgresJob.Id);
        Assert.NotNull(jobFromPostgres);
        Assert.Equal(postgresJob.Id, jobFromPostgres!.id);

        dynamic? postgresMessageFromSqlServer = await Fetch.MessageFromSqlServerAsync(postgresMessage.Id);
        Assert.Null(postgresMessageFromSqlServer);

        dynamic? sqlServerMessageFromPostgres = await Fetch.MessageFromPostgresAsync(sqlServerMessage.Id);
        Assert.Null(sqlServerMessageFromPostgres);
    }

    public void Dispose()
    {
        _serviceScope?.Dispose();
    }
}