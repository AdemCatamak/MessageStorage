using System;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.Repositories
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class SqlServerJobRepository_UpdateStatusAsync_Test
    {
        private readonly SqlServerRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "job_repository_updateasync_testdb";
        private readonly SqlServerInfraFixture _sqlServerInfraFixture;

        public SqlServerJobRepository_UpdateStatusAsync_Test(SqlServerInfraFixture sqlServerInfraFixture)
        {
            _sqlServerInfraFixture = sqlServerInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
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

            await using var connection = new SqlConnection(_sqlServerInfraFixture.ConnectionString);
            string jobCountScript = $"select * from {SCHEMA}.jobs where id = @id";
            dynamic? jobFromDb = await connection.QueryFirstAsync(jobCountScript, param: new {id = job.Id});

            Assert.Equal(jobId, jobFromDb.Id);
            Assert.Equal((int) JobStatus.Completed, jobFromDb.JobStatus);
            Assert.Equal(JobStatus.Completed.ToString(), jobFromDb.LastOperationInfo);
            AssertThat.LessThan((DateTime) jobFromDb.LastOperationTime, lastOperationTime);
        }
    }
}