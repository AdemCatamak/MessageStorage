using System;
using System.Threading.Tasks;
using MassTransit;
using MessageStorage.Integration.MassTransit.FunctionalTest.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.Integration.MassTransit.FunctionalTest
{
    [Collection(TestServerFixture.FIXTURE_KEY)]
    public class PublishIntegrationEventTest
    {
        private readonly TestServerFixture _testServerFixture;

        public PublishIntegrationEventTest(TestServerFixture testServerFixture)
        {
            _testServerFixture = testServerFixture;
        }


        private static bool _isMessagePublished = false;

        [Fact]
        public async Task When_IIntegrationEventAdded__MessageShouldBePublished()
        {
            using IServiceScope scope = _testServerFixture.GetServiceScope();
            IServiceProvider provider = scope.ServiceProvider;
            var outboxClient = provider.GetRequiredService<IMessageStorageClient>();

            EntityCreatedEvent entityCreatedEvent = new EntityCreatedEvent(Guid.NewGuid(), DateTime.UtcNow);
            await outboxClient.AddMessageAsync(entityCreatedEvent);

            await AsyncHelper.WaitFor(_testServerFixture.WaitAfterJobNotHandled, 2);

            Assert.True(_isMessagePublished);
        }

        public class EntityCreatedEvent : IIntegrationEvent
        {
            public Guid Id { get; private set; }
            public DateTime CreatedOn { get; private set; }

            public EntityCreatedEvent(Guid id, DateTime createdOn)
            {
                Id = id;
                CreatedOn = createdOn;
            }
        }

        public class EntityCreatedEvent_Consumer : IConsumer<EntityCreatedEvent>
        {
            public Task Consume(ConsumeContext<EntityCreatedEvent> context)
            {
                EntityCreatedEvent payload = context.Message;
                if (Guid.Empty == payload.Id) throw new ArgumentNullException(nameof(payload.Id));
                if (default == payload.CreatedOn) throw new ArgumentNullException(nameof(payload.CreatedOn));

                _isMessagePublished = true;
                return Task.CompletedTask;
            }
        }
    }
}