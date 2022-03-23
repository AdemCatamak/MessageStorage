using System;
using System.Threading.Tasks;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Postgres.IntegrationTest.Fixtures.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.MonitorClientTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class GetJobCountTest : IDisposable
{
    private readonly TestServerFixture _fixture;

    private readonly IServiceScope _serviceScope;
    private readonly IMessageStorageClient _messageStorageClient;
    private readonly IMonitorClient _monitorClient;

    public GetJobCountTest(TestServerFixture fixture)
    {
        _fixture = fixture;

        _serviceScope = _fixture.GetServiceScope();
        _messageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
        _monitorClient = _serviceScope.ServiceProvider.GetRequiredService<IMonitorClient>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenGetJobCountMethodIsUsed_ResponseShouldBeNotNull()
    {
        var basicMessage = new BasicMessage("some-message");
        await _messageStorageClient.AddMessageAsync(basicMessage);

        await AsyncHelper.WaitAsync();

        int completedJobCount = await _monitorClient.GetJobCountAsync(JobStatus.Completed);
        Assert.True(completedJobCount > 0);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}