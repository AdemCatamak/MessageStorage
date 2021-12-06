using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MySql.IntegrationTest.Checks;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using MessageStorage.MySql.Migrations;
using MySqlConnector;
using Xunit;

namespace MessageStorage.MySql.IntegrationTest.Repositories
{
    public class MySqlJobRepository_SetFirstQueuedJobToInProgressAsync_Test : IClassFixture<MySqlInfraFixture>
    {
        private readonly MySqlRepositoryFactory _repositoryFactory;
        private readonly MySqlInfraFixture _mySqlInfraFixture;
        private string Schema => _mySqlInfraFixture.Database;

        public MySqlJobRepository_SetFirstQueuedJobToInProgressAsync_Test(MySqlInfraFixture mySqlInfraFixture)
        {
            _mySqlInfraFixture = mySqlInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, Schema);
            _repositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
            executor.Execute();
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

            var connection = new MySqlConnection(_mySqlInfraFixture.ConnectionString);
            string jobScript = $"select job_status from {Schema}.jobs where id = @id";
            dynamic? jobStatusProperty = await connection.QueryFirstAsync(jobScript, new { id = job.Id });
            Assert.Equal((int)JobStatus.InProgress, jobStatusProperty.job_status);
        }
    }
}