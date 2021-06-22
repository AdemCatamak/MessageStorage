using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.SqlServer.IntegrationTest.Checks;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.Repositories
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class SqlServerJobRepository_SetFirstQueuedJobToInProgressAsync_Test
    {
        private readonly SqlServerRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "job_repository_sfqjtipa_testdb";
        private readonly SqlServerInfraFixture _sqlServerInfraFixture;

        private readonly DbChecks _dbChecks;

        public SqlServerJobRepository_SetFirstQueuedJobToInProgressAsync_Test(SqlServerInfraFixture sqlServerInfraFixture)
        {
            _sqlServerInfraFixture = sqlServerInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
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

            var connection = new SqlConnection(_sqlServerInfraFixture.ConnectionString);
            string jobScript = $"select JobStatus from {SCHEMA}.jobs where id = @id";
            dynamic? jobStatusProperty = await connection.QueryFirstAsync(jobScript, new {id = job.Id});
            Assert.Equal((int) JobStatus.InProgress, jobStatusProperty.JobStatus);
        }
    }
}