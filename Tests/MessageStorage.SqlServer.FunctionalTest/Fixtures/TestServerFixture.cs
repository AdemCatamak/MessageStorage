using System;
using MessageStorage.AspNetCore;
using MessageStorage.DependencyInjection;
using MessageStorage.SqlServer.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MessageStorage.SqlServer.FunctionalTest.Fixtures
{
    [CollectionDefinition(TestServerFixture.FIXTURE_KEY)]
    public class TestServerFixtureDefinition : ICollectionFixture<TestServerFixture>
    {
    }

    public class TestServerFixture : IDisposable
    {
        public const string FIXTURE_KEY = "TestServerFixtureKey";
        public TimeSpan WaitAfterJobNotHandled { get; } = TimeSpan.FromSeconds(1);
        public readonly SqlServerInfraFixture SqlServerInfraFixture;

        private readonly IHost _testServer;

        public TestServerFixture()
        {
            SqlServerInfraFixture = new SqlServerInfraFixture();
            string sqlServerConnectionStr = SqlServerInfraFixture.ConnectionString;

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
            hostBuilder.UseEnvironment("Test")
                       .ConfigureWebHost(builder =>
                        {
                            builder.UseTestServer();
                            builder.ConfigureServices(collection =>
                            {
                                collection.AddForgetty(configurator =>
                                           {
                                               configurator.Register(this.GetType().Assembly);
                                               configurator.UseSqlServer(sqlServerConnectionStr);
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
            SqlServerInfraFixture?.Dispose();
            _testServer?.Dispose();
        }
    }
}