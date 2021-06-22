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
    public class SqlServerMessageRepository_AddAsync_Test
    {
        private readonly SqlServerRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "message_repository_addasync_testdb";
        private readonly SqlServerInfraFixture _sqlServerInfraFixture;

        private readonly DbChecks _dbChecks;

        public SqlServerMessageRepository_AddAsync_Test(SqlServerInfraFixture sqlServerInfraFixture)
        {
            _sqlServerInfraFixture = sqlServerInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _dbChecks = new DbChecks(repositoryConfiguration.ConnectionString, repositoryConfiguration.Schema);
        }

        [Fact]
        public async Task When_AddAsyncExecuted__MessageShouldBePersisted()
        {
            Message message = new Message("some-payload");

            IMessageRepository messageRepository = _repositoryFactory.CreateMessageRepository();
            await messageRepository.AddAsync(message);

            bool doesMessageExist = await _dbChecks.CheckMessageIsExistAsync(message.Id);
            Assert.True(doesMessageExist);
        }

        [Fact]
        public async Task When_AddAsyncExecuted_But_ExternalTransactionRollback__MessageShouldNotBePersisted()
        {
            Message message = new Message("some-payload");

            await using var connection = new SqlConnection(_sqlServerInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                IMessageStorageTransaction messageStorageTransaction = transaction.GetMessageStorageTransaction();

                IMessageRepository messageRepository = _repositoryFactory.CreateMessageRepository(messageStorageTransaction);
                await messageRepository.AddAsync(message);
            }

            bool doesMessageExist = await _dbChecks.CheckMessageIsExistAsync(message.Id);
            Assert.False(doesMessageExist);
        }
    }
}