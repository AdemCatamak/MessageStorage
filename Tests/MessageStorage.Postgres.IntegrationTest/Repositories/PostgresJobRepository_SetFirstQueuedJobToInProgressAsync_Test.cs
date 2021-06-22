using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.Postgres.IntegrationTest.Checks;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Postgres.Migrations;
using Npgsql;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.Repositories
{
    [Collection(PostgresInfraFixture.FIXTURE_KEY)]
    public class PostgresJobRepository_SetFirstQueuedJobToInProgressAsync_Test
    {
        private readonly PostgresRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "job_repository_sfqjtipa_testdb";
        private readonly PostgresInfraFixture _postgresInfraFixture;

        private readonly DbChecks _dbChecks;

        public PostgresJobRepository_SetFirstQueuedJobToInProgressAsync_Test(PostgresInfraFixture postgresInfraFixture)
        {
            _postgresInfraFixture = postgresInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(postgresInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new PostgresRepositoryFactory(repositoryConfiguration);

            var executor = new PostgresMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _dbChecks = new DbChecks(repositoryConfiguration.ConnectionString, repositoryConfiguration.Schema);
        }


        [Fact]
        public async Task When_ThereIsNoJobInQueue_SetFirstQueuedJobInProgressAsync__ResponseShouldBeEmpty()
        {
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            Job? job = await jobRepository.SetFirstQueuedJobToInProgressAsync();

            Assert.Null(job);
        }

        [Fact]
        public async Task When_ThereIsJobInQueue_without_Message__ResponseShouldBeEmpty()
        {
            var message = new Message("some-payload");
            Job seedJob = new Job(message, "some-handler");
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.AddAsync(seedJob);

            Job? job = await jobRepository.SetFirstQueuedJobToInProgressAsync();

            Assert.Null(job);
        }

        [Fact]
        public async Task When_ThereIsJobInQueue_SetFirstQueuedJobInProgressAsync__JobStatusIsChanged()
        {
            var message = new Message("some-payload");
            IMessageRepository messageRepository = _repositoryFactory.CreateMessageRepository();
            await messageRepository.AddAsync(message);

            Job seedJob = new Job(message, "some-handler");
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.AddAsync(seedJob);

            Job? jobInProgress = await jobRepository.SetFirstQueuedJobToInProgressAsync();

            Assert.NotNull(jobInProgress);
            Job job = jobInProgress!;
            Assert.Equal(JobStatus.InProgress, job.JobStatus);

            var connection = new NpgsqlConnection(_postgresInfraFixture.ConnectionString);
            string jobScript = $"select job_status from {SCHEMA}.jobs where id = @id";
            dynamic? jobStatusProperty = await connection.QueryFirstAsync(jobScript, new {id = job.Id});
            Assert.Equal((int) JobStatus.InProgress, jobStatusProperty.job_status);
        }
    }
}