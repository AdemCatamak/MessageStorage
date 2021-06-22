using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using MessageStorage.Integration.MassTransit.FunctionalTest.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.Integration.MassTransit.FunctionalTest
{
    [Collection(TestServerFixture.FIXTURE_KEY)]
    public class PublishIntegrationCommandTest
    {
        private readonly TestServerFixture _testServerFixture;

        public PublishIntegrationCommandTest(TestServerFixture testServerFixture)
        {
            _testServerFixture = testServerFixture;
        }


        private static bool _isMessagePublished = false;

        [Fact]
        public async Task When_IIntegrationCommandAdded__MessageShouldBeSent()
        {
            using IServiceScope scope = _testServerFixture.GetServiceScope();
            IServiceProvider provider = scope.ServiceProvider;
            var outboxClient = provider.GetRequiredService<IMessageStorageClient>();

            UpdateSameEntityCommand updateSameEntityCommand = new UpdateSameEntityCommand("some-value");
            await outboxClient.AddMessageAsync(updateSameEntityCommand);

            await AsyncHelper.WaitFor(_testServerFixture.WaitAfterJobNotHandled, 2);

            Assert.True(_isMessagePublished);
        }

        public class UpdateSameEntityCommand : IIntegrationCommand
        {
            public string SomeValue { get; private set; }

            public UpdateSameEntityCommand(string someValue)
            {
                SomeValue = someValue;
            }
        }

        public class UpdateSomeEntityCommand_Consumer : IConsumer<UpdateSameEntityCommand>
        {
            public Task Consume(ConsumeContext<UpdateSameEntityCommand> context)
            {
                UpdateSameEntityCommand payload = context.Message;
                if (string.IsNullOrEmpty(payload.SomeValue))
                {
                    throw new ArgumentNullException(nameof(payload.SomeValue));
                }

                _isMessagePublished = true;
                return Task.CompletedTask;
            }
        }

        public class UpdateSomeEntityCommand_ConsumerDefinition : ConsumerDefinition<UpdateSomeEntityCommand_Consumer>
        {
            public const string QueueName = "update-some-entity";

            public UpdateSomeEntityCommand_ConsumerDefinition()
            {
                Endpoint(configurator => { configurator.Name = QueueName; });
            }
        }
    }
}