using System;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Postgres.Migrations;
using Npgsql;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.Repositories
{
    [Collection(PostgresInfraFixture.FIXTURE_KEY)]
    public class PostgresJobRepository_UpdateStatusAsync_Test
    {
        private readonly PostgresRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "job_repository_updateasync_testdb";
        private readonly PostgresInfraFixture _postgresInfraFixture;

        public PostgresJobRepository_UpdateStatusAsync_Test(PostgresInfraFixture postgresInfraFixture)
        {
            _postgresInfraFixture = postgresInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(postgresInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new PostgresRepositoryFactory(repositoryConfiguration);

            var executor = new PostgresMigrationExecutor(repositoryConfiguration);
            executor.Execute();
        }

        [Fact]
        public async Task When_UpdateAsyncExecuted__Status_LastOperationInfo_LastOperationTime_ShouldBeChanged()
        {
            var jobId = Guid.NewGuid();
            DateTime lastOperationTime = DateTime.Parse("2021-01-01");
            const string lastOperationInfo = "last-operation-info";

            Job job = new Job(jobId, new Message("some-payload"), "some-handler", JobStatus.Queued, DateTime.UtcNow, lastOperationTime, lastOperationInfo, 5, DateTime.UtcNow);
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.AddAsync(job);

            job.SetCompleted();
            await jobRepository.UpdateJobStatusAsync(job);

            await using var connection = new NpgsqlConnection(_postgresInfraFixture.ConnectionString);
            string jobCountScript = $"select * from {SCHEMA}.jobs where id = @id";
            var jobFromDb = await connection.QueryFirstAsync(jobCountScript, param: new {id = job.Id});

            Assert.Equal(jobId, jobFromDb.id);
            Assert.Equal((int) JobStatus.Completed, jobFromDb.job_status);
            Assert.Equal(JobStatus.Completed.ToString(), jobFromDb.last_operation_info);
            AssertThat.GreaterThan(lastOperationTime, (DateTime) jobFromDb.last_operation_time);
        }
    }
}