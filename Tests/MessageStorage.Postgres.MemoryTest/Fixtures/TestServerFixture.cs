using System;
using MessageStorage.Extensions;
using MessageStorage.Postgres.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestUtility.DbUtils;

namespace MessageStorage.Postgres.MemoryTest.Fixtures;

public class TestServerFixture : IDisposable
{
    public const string FIXTURE_KEY = "PostgresIntegrationTest.TestServerFixtureKey";
    public string ConnectionStr => _postgresFixture.ConnectionStr;

    private readonly PostgresFixture _postgresFixture;
    private readonly IHost _testServer;

    public TestServerFixture()
    {
        _postgresFixture = new PostgresFixture();
        Db.SetPostgresConnectionStr(_postgresFixture.ConnectionStr);


        IHostBuilder? hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.UseEnvironment("Test")
                   .ConfigureWebHost(builder =>
                    {
                        builder.UseTestServer();
                        builder.ConfigureServices(collection =>
                        {
                            collection.AddMessageStorage(optionBuilder =>
                            {
                                optionBuilder.RegisterHandlers(GetType().Assembly);
                                optionBuilder.UsePostgres(_postgresFixture.ConnectionStr);
                            });
                        });
                        builder.Configure(_ => { });
                        builder.ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole()
                                                                                 .SetMinimumLevel(LogLevel.Debug));
                    });
        _testServer = hostBuilder.Build();
        _testServer.Start();
    }

    public IServiceScope GetServiceScope()
    {
        IServiceScope serviceScope = _testServer.Services.CreateScope();
        return serviceScope;
    }

    public void Dispose()
    {
        _testServer?.Dispose();
        _postgresFixture?.Dispose();
    }
}