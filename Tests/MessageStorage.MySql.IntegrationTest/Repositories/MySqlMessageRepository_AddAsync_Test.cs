using System.Data;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MySql.DbClient;
using MessageStorage.MySql.Extension;
using MessageStorage.MySql.IntegrationTest.Checks;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using MessageStorage.MySql.Migrations;
using MySqlConnector;
using Xunit;

namespace MessageStorage.MySql.IntegrationTest.Repositories
{
    public class MySqlMessageRepository_AddAsync_Test : IClassFixture<MySqlInfraFixture>
    {
        private readonly MySqlRepositoryFactory _repositoryFactory;
        private readonly MySqlInfraFixture _mySqlInfraFixture;
        private readonly DbChecks _dbChecks;

        private string Schema => _mySqlInfraFixture.Database;

        public MySqlMessageRepository_AddAsync_Test(MySqlInfraFixture mySqlInfraFixture)
        {
            _mySqlInfraFixture = mySqlInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, Schema);
            _repositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _dbChecks = new DbChecks(repositoryConfiguration.ConnectionString, repositoryConfiguration.Schema);
        }

        [Fact]
        public async Task When_AddAsyncExecuted__MessageShouldBePersisted()
        {
            Message message = new Message("some-payload");

            IMessageRepository messageRepository = _repositoryFactory.CreateMessageRepository();
            await messageRepository.AddAsync(message);

            bool doesMessageIsExist = await _dbChecks.DoesMessageIsExistAsync(message.Id);
            Assert.True(doesMessageIsExist);
        }

        [Fact]
        public async Task When_AddAsyncExecuted_But_ExternalTransactionRollback__MessageShouldNotBePersisted()
        {
            Message message = new Message("some-payload");

            await using var connection = new MySqlConnection(_mySqlInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (MySqlTransaction? transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                IMySqlMessageStorageTransaction messageStorageTransaction = transaction.GetMessageStorageTransaction();

                IMessageRepository messageRepository = _repositoryFactory.CreateMessageRepository(messageStorageTransaction);
                await messageRepository.AddAsync(message);
            }

            bool doesMessageIsExist = await _dbChecks.DoesMessageIsExistAsync(message.Id);
            Assert.False(doesMessageIsExist);
        }
    }
}