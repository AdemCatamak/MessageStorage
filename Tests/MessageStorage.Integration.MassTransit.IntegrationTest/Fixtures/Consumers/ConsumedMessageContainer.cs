using System.Collections.Concurrent;

namespace MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures.Consumers;

public static class ConsumedMessageContainer
{
    public static ConcurrentBag<string> ConsumedMessageId = new();
}