using System;
using MassTransit;
using MessageStorage.AspNetCore;
using MessageStorage.DependencyInjection;
using MessageStorage.Postgres.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace MessageStorage.Integration.MassTransit.FunctionalTest.Fixtures
{
    [CollectionDefinition(TestServerFixture.FIXTURE_KEY)]
    public class TestServerFixtureDefinition : ICollectionFixture<TestServerFixture>
    {
    }

    public class TestServerFixture : IDisposable
    {
        public const string FIXTURE_KEY = "TestServerFixtureKey";
        public TimeSpan WaitAfterJobNotHandled { get; } = TimeSpan.FromSeconds(1);
        public readonly PostgresInfraFixture PostgresInfraFixture;
        public readonly RabbitMqInfraFixture RabbitMqInfraFixture;

        private readonly IHost _testServer;

        public TestServerFixture()
        {
            RabbitMqInfraFixture = new RabbitMqInfraFixture();
            PostgresInfraFixture = new PostgresInfraFixture();
            string postgresConnectionStr = PostgresInfraFixture.ConnectionString;

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
            hostBuilder.UseEnvironment("Test")
                       .ConfigureWebHost(builder =>
                        {
                            builder.UseTestServer();
                            builder.ConfigureServices(services =>
                            {
                                services.AddMassTransit(x =>
                                {
                                    x.SetKebabCaseEndpointNameFormatter();
                                    x.AddConsumers(this.GetType().Assembly);
                                    x.UsingRabbitMq((context, cfg) =>
                                    {
                                        cfg.Host(RabbitMqInfraFixture.Host, "/", h =>
                                        {
                                            h.Username(RabbitMqInfraFixture.Username);
                                            h.Password(RabbitMqInfraFixture.Password);
                                            h.UseCluster(clusterConfigurator => { clusterConfigurator.Node($"{RabbitMqInfraFixture.Host}:{RabbitMqInfraFixture.Port}"); });
                                        });

                                        cfg.ConfigureEndpoints(context);
                                    });
                                });
                                services.AddMassTransitHostedService(true);
                                EndpointConvention.Map<PublishIntegrationCommandTest.UpdateSameEntityCommand>(new Uri($"rabbitmq://{RabbitMqInfraFixture.Host}:{RabbitMqInfraFixture.Port}/{PublishIntegrationCommandTest.UpdateSomeEntityCommand_ConsumerDefinition.QueueName}"));

                                services.AddForgetty(configurator =>
                                         {
                                             configurator.UsePostgres(postgresConnectionStr);
                                             configurator.Register(GetType().Assembly);
                                             configurator.RegisterMassTransitMessageHandlers();
                                         })
                                        .AddMessageStoragePrerequisiteExecutor()
                                        .AddMessageStorageJobDispatcherHostedService(waitAfterJobNotHandled: WaitAfterJobNotHandled);
                            });
                            builder.Configure(applicationBuilder => { });
                            builder.ConfigureLogging(loggingBuilder =>
                            {
                                loggingBuilder.AddConsole();
                                loggingBuilder.SetMinimumLevel(LogLevel.Error);
                            });
                        });

            _testServer = hostBuilder.Build();
            _testServer.Start();
        }

        public IServiceScope GetServiceScope()
        {
            var serviceScope = _testServer.Services.CreateScope();
            return serviceScope;
        }

        public void Dispose()
        {
            PostgresInfraFixture?.Dispose();
            RabbitMqInfraFixture?.Dispose();
            _testServer?.Dispose();
        }
    }
}