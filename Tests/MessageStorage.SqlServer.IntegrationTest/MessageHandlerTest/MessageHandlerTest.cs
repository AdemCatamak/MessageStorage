using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.IntegrationTest.Fixtures.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.MessageHandlerTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class MessageHandlerTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IMessageStorageClient _messageStorageClient;

    public MessageHandlerTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _messageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenThereIsNoError_JobShouldBeMarkedAsCompleted()
    {
        var basicMessage = new BasicMessage("some-message");
        (_, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(basicMessage, CancellationToken.None);

        Assert.Single(jobs);
        Job job = jobs.First();

        await AsyncHelper.WaitAsync();

        dynamic? jobFromDb = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDb);
        Assert.Equal((int)JobStatus.Succeeded, jobFromDb!.JobStatus);
    }

    [Fact(Timeout = 3000)]
    public async Task WhenThereIsError_JobShouldBeMarkedAsFailed()
    {
        var throwExMessage = new ThrowExMessage("some-message");
        (_, List<Job> jobs) = await _messageStorageClient.AddMessageAsync(throwExMessage);

        Assert.Single(jobs);
        Job job = jobs.First();

        await AsyncHelper.WaitAsync();

        dynamic? jobFromDb = await Fetch.JobFromSqlServerAsync(job.Id);
        Assert.NotNull(jobFromDb);
        Assert.Equal((int)JobStatus.Failed, jobFromDb!.JobStatus);
        Assert.Equal(throwExMessage.ExceptionMessage, jobFromDb.LastOperationInfo);
    }

    public void Dispose()
    {
        _serviceScope?.Dispose();
    }
}