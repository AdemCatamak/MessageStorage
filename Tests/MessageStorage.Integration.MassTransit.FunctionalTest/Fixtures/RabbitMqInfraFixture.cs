using System;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.MessageBrokers;
using DotNet.Testcontainers.Containers.Modules.MessageBrokers;
using TestUtility;

namespace MessageStorage.Integration.MassTransit.FunctionalTest.Fixtures
{
    public class RabbitMqInfraFixture : IDisposable
    {
        public string Host { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Port { get; private set; }


        private readonly RabbitMqTestcontainer _rabbitMqTestContainer;

        public RabbitMqInfraFixture()
        {
            Host = "127.0.0.1";
            Username = "guest";
            Password = "guest";
            Port = NetworkUtility.GetAvailablePort();

            ITestcontainersBuilder<RabbitMqTestcontainer> rabbitMqTestContainerBuilder
                = new TestcontainersBuilder<RabbitMqTestcontainer>()
                   .WithMessageBroker(new RabbitMqTestcontainerConfiguration
                    {
                        Username = Username,
                        Password = Password,
                        Port = Port
                    });

            _rabbitMqTestContainer = rabbitMqTestContainerBuilder.Build();
            _rabbitMqTestContainer.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _rabbitMqTestContainer?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}