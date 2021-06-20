using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.SqlServer.Extension;
using MessageStorage.SqlServer.IntegrationTest.Checks;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.Repositories
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class SqlServerJobRepository_AddAsync_Test
    {
        private readonly SqlServerRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "job_repository_addasync_testdb";
        private readonly SqlServerInfraFixture _sqlServerInfraFixture;

        private readonly DbChecks _dbChecks;

        public SqlServerJobRepository_AddAsync_Test(SqlServerInfraFixture sqlServerInfraFixture)
        {
            _sqlServerInfraFixture = sqlServerInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _dbChecks = new DbChecks(repositoryConfiguration.ConnectionString, repositoryConfiguration.Schema);
        }


        [Fact]
        public async Task When_AddAsyncExecuted__JobShouldBePersisted()
        {
            Job job = new Job(new Message("some-payload"), "some-handler");

            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.AddAsync(job);

            bool doesJobExist = await _dbChecks.CheckJobIsExistAsync(job.Id);
            Assert.True(doesJobExist);
        }

        [Fact]
        public async Task When_AddAsyncExecuted_But_ExternalTransactionRollback__JobShouldNotBePersisted()
        {
            Job job = new Job(new Message("some-payload"), "some-handler");

            await using SqlConnection connection = new SqlConnection(_sqlServerInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                IMessageStorageTransaction messageStorageTransaction = transaction.GetMessageStorageTransaction();

                IJobRepository jobRepository = _repositoryFactory.CreateJobRepository(messageStorageTransaction);
                await jobRepository.AddAsync(job);
            }

            bool doesJobExist = await _dbChecks.CheckJobIsExistAsync(job.Id);
            Assert.False(doesJobExist);
        }
    }
}