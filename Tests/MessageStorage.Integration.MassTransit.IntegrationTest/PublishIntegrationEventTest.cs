using System;
using System.Threading.Tasks;
using MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures;
using MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures.Consumers;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.Integration.MassTransit.IntegrationTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class PublishIntegrationEventTest : IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly IMessageStorageClient _messageStorageClient;

    public PublishIntegrationEventTest(TestServerFixture fixture)
    {
        _serviceScope = fixture.GetServiceScope();
        _messageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenIIntegrationEventAdded_MessageShouldBePublished()
    {
        var basicIntegrationEvent = new BasicIntegrationEvent(Guid.NewGuid().ToString());
        await _messageStorageClient.AddMessageAsync(basicIntegrationEvent);

        await AsyncHelper.WaitAsync();

        Assert.Contains(basicIntegrationEvent.Guid, ConsumedMessageContainer.ConsumedMessageId);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}