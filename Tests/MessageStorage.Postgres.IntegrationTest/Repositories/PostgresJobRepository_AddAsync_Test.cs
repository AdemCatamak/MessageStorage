using System.Data;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.Postgres.DbClient;
using MessageStorage.Postgres.Extension;
using MessageStorage.Postgres.IntegrationTest.Checks;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Postgres.Migrations;
using Npgsql;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.Repositories
{
    [Collection(PostgresInfraFixture.FIXTURE_KEY)]
    public class PostgresJobRepository_AddAsync_Test
    {
        private readonly PostgresRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "job_repository_addasync_testdb";
        private readonly PostgresInfraFixture _postgresInfraFixture;
        private readonly DbChecks _dbChecks;

        public PostgresJobRepository_AddAsync_Test(PostgresInfraFixture postgresInfraFixture)
        {
            _postgresInfraFixture = postgresInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(postgresInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new PostgresRepositoryFactory(repositoryConfiguration);

            var executor = new PostgresMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _dbChecks = new DbChecks(repositoryConfiguration.ConnectionString, repositoryConfiguration.Schema);
        }

        [Fact]
        public async Task When_AddAsyncExecuted__JobShouldBePersisted()
        {
            Job job = new Job(new Message("some-payload"), "some-handler");

            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.AddAsync(job);

            bool doesJobExist = await _dbChecks.DoesJobIsExistAsync(job.Id);
            Assert.True(doesJobExist);
        }

        [Fact]
        public async Task When_AddAsyncExecuted_But_ExternalTransactionRollback__JobShouldNotBePersisted()
        {
            Job job = new Job(new Message("some-payload"), "some-handler");

            await using var connection = new NpgsqlConnection(_postgresInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                IPostgresMessageStorageTransaction messageStorageTransaction = transaction.GetMessageStorageTransaction();

                IJobRepository jobRepository = _repositoryFactory.CreateJobRepository(messageStorageTransaction);
                await jobRepository.AddAsync(job);
            }

            bool doesJobExist = await _dbChecks.DoesJobIsExistAsync(job.Id);
            Assert.False(doesJobExist);
        }
    }
}