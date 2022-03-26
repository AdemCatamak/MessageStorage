using System;
using MessageStorage.Extensions;
using MessageStorage.SqlServer.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestUtility.DbUtils;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.Fixtures;

[CollectionDefinition(TestServerFixture.FIXTURE_KEY)]
public class TestServerFixtureDefinition : ICollectionFixture<TestServerFixture>
{
}

public class TestServerFixture : IDisposable
{
    public const string FIXTURE_KEY = "SqlServerIntegrationTest.TestServerFixtureKey";
    public string ConnectionStr => _sqlServerFixture.ConnectionStr;

    private readonly SqlServerFixture _sqlServerFixture;
    private readonly IHost _testServer;

    public TestServerFixture()
    {
        _sqlServerFixture = new SqlServerFixture();
        Db.SetSqlServerConnectionStr(_sqlServerFixture.ConnectionStr);

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
                                optionBuilder.UseSqlServer(_sqlServerFixture.ConnectionStr);
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
        IServiceScope? serviceScope = _testServer.Services.CreateScope();
        return serviceScope;
    }

    public void Dispose()
    {
        _testServer?.Dispose();
        _sqlServerFixture?.Dispose();
    }
}