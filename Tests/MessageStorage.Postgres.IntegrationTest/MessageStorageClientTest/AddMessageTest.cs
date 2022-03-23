using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Postgres.IntegrationTest.Fixtures.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.MessageStorageClientTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class AddMessageTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IMessageStorageClient _messageStorageClient;

    public AddMessageTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _messageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
    }

    [Fact(Timeout = 1000)]
    public async Task WhenThereIsNoCompatibleMessageHandler_ThereShouldBeNoJob()
    {
        Message message;
        var handlerlessMessage = new HandlerlessMessage("some message");
        (message, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(handlerlessMessage);

        Assert.Empty(jobs);

        dynamic? messageFromDb = await Fetch.MessageFromPostgresAsync(message.Id);
        Assert.NotNull(messageFromDb);
        Assert.Equal(message.Id, messageFromDb!.id);
    }

    [Fact(Timeout = 1000)]
    public async Task WhenThereIsCompatibleMessageHandler_JobListShouldNotBeEmpty()
    {
        Message message;
        var basicMessage = new BasicMessage("some-message");
        (message, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage);

        Assert.Single(jobs);
        Job job = jobs.First();

        dynamic? messageFromDb = await Fetch.MessageFromPostgresAsync(message.Id);
        Assert.NotNull(messageFromDb);
        Assert.Equal(message.Id, messageFromDb!.id);

        dynamic? jobFromDb = await Fetch.JobFromPostgresAsync(job.Id);
        Assert.NotNull(jobFromDb);
        Assert.Equal(job.Id, jobFromDb!.id);
    }


    public void Dispose()
    {
        _serviceScope?.Dispose();
    }
}