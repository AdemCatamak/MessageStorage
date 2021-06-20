using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.Postgres.DbClient;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest
{
    [Collection(PostgresInfraFixture.FIXTURE_KEY)]
    public class PostgresRepositoryFactory_CreateConnection_Test
    {
        private readonly PostgresRepositoryFactory _sut;

        public PostgresRepositoryFactory_CreateConnection_Test(PostgresInfraFixture postgresInfraFixture)
        {
            _sut = new PostgresRepositoryFactory(new RepositoryConfiguration(postgresInfraFixture.ConnectionString, "CreateConnection_Test_Schema"));
        }

        [Fact]
        public async Task When_PostgresRepositoryFactoryCreateConnection__ConnectionHasAbleToExecuteScript()
        {
            using IPostgresMessageStorageConnection postgresMessageStorageConnection = _sut.CreateConnection();
            var results = (await postgresMessageStorageConnection.QueryAsync("Select now() at time zone 'utc' as now", null, CancellationToken.None))
               .ToList();

            Assert.NotEmpty(results);
            var dbResponse = (DateTime) results.First().now;

            DateTime now = DateTime.UtcNow;

            AssertThat.GreaterThan(dbResponse, now.AddMinutes(1));
            AssertThat.LessThan(dbResponse, now.AddMinutes(-1));
        }
    }
}