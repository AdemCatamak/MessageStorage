using System.Data;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MySql.Extension;
using MessageStorage.MySql.IntegrationTest.Checks;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using MessageStorage.MySql.Migrations;
using MySqlConnector;
using Xunit;

namespace MessageStorage.MySql.IntegrationTest.Repositories
{
    public class MySqlJobRepository_AddAsync_Test : IClassFixture<MySqlInfraFixture>
    {
        private readonly MySqlRepositoryFactory _repositoryFactory;
        private readonly MySqlInfraFixture _mySqlInfraFixture;
        private readonly DbChecks _dbChecks;
        
        private string Schema => _mySqlInfraFixture.Database;
        
        public MySqlJobRepository_AddAsync_Test(MySqlInfraFixture mySqlInfraFixture)
        {
            _mySqlInfraFixture = mySqlInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, Schema);
            _repositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
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

            await using var connection = new MySqlConnection(_mySqlInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                var messageStorageTransaction = transaction.GetMessageStorageTransaction();

                IJobRepository jobRepository = _repositoryFactory.CreateJobRepository(messageStorageTransaction);
                await jobRepository.AddAsync(job);
            }

            bool doesJobExist = await _dbChecks.DoesJobIsExistAsync(job.Id);
            Assert.False(doesJobExist);
        }
    }
}