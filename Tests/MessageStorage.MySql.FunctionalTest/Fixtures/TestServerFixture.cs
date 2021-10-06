using System;
using MessageStorage.AspNetCore;
using MessageStorage.DependencyInjection;
using MessageStorage.MySql.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MessageStorage.MySql.FunctionalTest.Fixtures
{
    [CollectionDefinition(TestServerFixture.FIXTURE_KEY)]
    public class TestServerFixtureDefinition : ICollectionFixture<TestServerFixture>
    {
    }

    public class TestServerFixture : IDisposable
    {
        public const string FIXTURE_KEY = "TestServerFixtureKey";
        public TimeSpan WaitAfterJobNotHandled { get; } = TimeSpan.FromSeconds(1);
        public readonly MySqlInfraFixture MySqlInfraFixture;

        private readonly IHost _testServer;

        public TestServerFixture()
        {
            MySqlInfraFixture = new MySqlInfraFixture();
            string connectionString = MySqlInfraFixture.ConnectionString;

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
            hostBuilder.UseEnvironment("Test")
                       .ConfigureWebHost(builder =>
                        {
                            builder.UseTestServer();
                            builder.ConfigureServices(collection =>
                            {
                                collection.AddMessageStorage(configurator =>
                                           {
                                               configurator.Register(this.GetType().Assembly);
                                               configurator.UseMySql(connectionString);
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
            MySqlInfraFixture?.Dispose();
            _testServer?.Dispose();
        }
    }
}