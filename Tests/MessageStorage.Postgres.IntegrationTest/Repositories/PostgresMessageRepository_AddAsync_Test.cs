using System.Data;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.Postgres.DbClient;
using MessageStorage.Postgres.Extension;
using MessageStorage.Postgres.IntegrationTest.Checks;
using MessageStorage.Postgres.IntegrationTest.Fixtures;
using MessageStorage.Postgres.Migrations;
using Npgsql;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.Repositories
{
    [Collection(PostgresInfraFixture.FIXTURE_KEY)]
    public class PostgresMessageRepository_AddAsync_Test
    {
        private readonly PostgresRepositoryFactory _repositoryFactory;

        private const string SCHEMA = "message_repository_addasync_testdb";
        private readonly PostgresInfraFixture _postgresInfraFixture;

        private readonly DbChecks _dbChecks;

        public PostgresMessageRepository_AddAsync_Test(PostgresInfraFixture postgresInfraFixture)
        {
            _postgresInfraFixture = postgresInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(postgresInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new PostgresRepositoryFactory(repositoryConfiguration);

            var executor = new PostgresMigrationExecutor(repositoryConfiguration);
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

            await using var connection = new NpgsqlConnection(_postgresInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                IPostgresMessageStorageTransaction messageStorageTransaction = transaction.GetMessageStorageTransaction();

                IMessageRepository messageRepository = _repositoryFactory.CreateMessageRepository(messageStorageTransaction);
                await messageRepository.AddAsync(message);
            }

            bool doesMessageIsExist = await _dbChecks.DoesMessageIsExistAsync(message.Id);
            Assert.False(doesMessageIsExist);
        }
    }
}