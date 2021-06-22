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

        private readonly IHost _testServer;

        public TestServerFixture()
        {
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
                                    x.UsingInMemory((context, cfg) =>
                                    {
                                        cfg.Host();
                                        cfg.ConfigureEndpoints(context);
                                    });
                                });
                                services.AddMassTransitHostedService(true);
                                EndpointConvention.Map<PublishIntegrationCommandTest.UpdateSameEntityCommand>(new Uri($"queue:{PublishIntegrationCommandTest.UpdateSomeEntityCommand_ConsumerDefinition.QueueName}"));

                                services.AddMessageStorage(configurator =>
                                         {
                                             configurator.UsePostgres(postgresConnectionStr);
                                             configurator.Register(GetType().Assembly);
                                             configurator.RegisterMassTransitMessageHandlers();
                                         })
                                        .AddMessageStoragePrerequisiteExecutor()
                                        .AddMessageStorageJobDispatcher(waitAfterJobNotHandled: WaitAfterJobNotHandled);
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
            _testServer?.Dispose();
        }
    }
}