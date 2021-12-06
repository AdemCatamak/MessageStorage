using System;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using MessageStorage.MySql.Migrations;
using MySqlConnector;
using TestUtility;
using Xunit;

namespace MessageStorage.MySql.IntegrationTest.Repositories
{
    public class MySqlJobRepository_UpdateStatusAsync_Test : IClassFixture<MySqlInfraFixture>
    {
        private readonly MySqlRepositoryFactory _repositoryFactory;
        private readonly MySqlInfraFixture _mySqlInfraFixture;

        private string Schema => _mySqlInfraFixture.Database;

        public MySqlJobRepository_UpdateStatusAsync_Test(MySqlInfraFixture mySqlInfraFixture)
        {
            _mySqlInfraFixture = mySqlInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, Schema);
            _repositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
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

            await using var connection = new MySqlConnection(_mySqlInfraFixture.ConnectionString);
            string jobCountScript = $"select * from {Schema}.jobs where id = @id";
            var jobFromDb = await connection.QueryFirstAsync(jobCountScript, param: new { id = job.Id });

            Assert.Equal(jobId, jobFromDb.id);
            Assert.Equal((int)JobStatus.Completed, jobFromDb.job_status);
            Assert.Equal(JobStatus.Completed.ToString(), jobFromDb.last_operation_info);
            AssertThat.GreaterThan(lastOperationTime, (DateTime)jobFromDb.last_operation_time);
        }
    }
}