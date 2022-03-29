using System;
using System.Threading.Tasks;
using MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures;
using MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures.Consumers;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.Integration.MassTransit.IntegrationTest;

[Collection(TestServerFixture.FIXTURE_KEY)]
public class PublishIntegrationCommandTest : IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly IMessageStorageClient _messageStorageClient;

    public PublishIntegrationCommandTest(TestServerFixture fixture)
    {
        _serviceScope = fixture.GetServiceScope();
        _messageStorageClient = _serviceScope.ServiceProvider.GetRequiredService<IMessageStorageClient>();
    }

    [Fact(Timeout = 3000)]
    public async Task WhenIIntegrationCommandAdded_MessageShouldBeSent()
    {
        var basicIntegrationCommand = new BasicIntegrationCommand(Guid.NewGuid().ToString());
        await _messageStorageClient.AddMessageAsync(basicIntegrationCommand);

        await AsyncHelper.WaitAsync();

        Assert.Contains(basicIntegrationCommand.Guid, ConsumedMessageContainer.ConsumedMessageId);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}