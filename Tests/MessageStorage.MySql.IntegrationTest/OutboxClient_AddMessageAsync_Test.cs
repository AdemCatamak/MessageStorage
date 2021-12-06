using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MySql.Extension;
using MessageStorage.MySql.IntegrationTest.Checks;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using MessageStorage.MySql.Migrations;
using MySqlConnector;
using Xunit;

namespace MessageStorage.MySql.IntegrationTest
{
    public class OutboxClient_AddMessageAsync_Test : IClassFixture<MySqlInfraFixture>
    {
        private readonly IMessageStorageClient _sut;
        private readonly MySqlInfraFixture _mySqlInfraFixture;
        private readonly DbChecks _dbChecks;

        private string Schema => _mySqlInfraFixture.Database;

        public OutboxClient_AddMessageAsync_Test(MySqlInfraFixture mySqlInfraFixture)
        {
            _mySqlInfraFixture = mySqlInfraFixture;
            MessageHandlerContainer messageHandlerContainer = new MessageHandlerContainer();
            messageHandlerContainer.Register<StringMessageHandler>();
            messageHandlerContainer.Register(typeof(IntMessageHandler));
            messageHandlerContainer.Register<ObjMessageHandler>();

            IMessageHandlerProvider messageHandlerProvider = messageHandlerContainer.BuildMessageHandlerProvider();
            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, Schema);
            var postgresRepositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);
            _sut = new MessageStorageClient(messageHandlerProvider, postgresRepositoryFactory);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _dbChecks = new DbChecks(repositoryConfiguration.ConnectionString, repositoryConfiguration.Schema);
        }

        [Theory]
        [InlineData("string", 2)]
        [InlineData(1, 2)]
        [InlineData(true, 1)]
        public async Task When_AddMessageAsync__JobCountShouldBeEqualToAvailableMessageHandlerCount(object payload, int expectedJobCount)
        {
            var (_, jobs) = await _sut.AddMessageAsync(payload);
            var jobList = jobs.ToList();
            Assert.Equal(expectedJobCount, jobList.Count);
        }

        [Fact]
        public async Task When_AddMessageAsync__Message_and_Jobs_ShouldPersistedInDb()
        {
            var (message, jobs) = await _sut.AddMessageAsync("some-payload");
            var jobList = jobs.ToList();
            Assert.Equal(2, jobList.Count);

            bool doesMessageExist = await _dbChecks.DoesMessageIsExistAsync(message.Id);
            Assert.True(doesMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.DoesJobIsExistAsync(job.Id);
                Assert.True(doesJobExist);
            }
        }

        [Fact]
        public async Task When_AddMessageAsync_TransactionNoCommitted__Message_and_Jobs_ShouldNotPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            await using var connection = new MySqlConnection(_mySqlInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (MySqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                (message, jobs) = await _sut.AddMessageAsync("some-payload", transaction);
            }

            var jobList = jobs.ToList();

            Assert.NotNull(message);
            Assert.NotEmpty(jobList);

            bool doesMessageExist = await _dbChecks.DoesMessageIsExistAsync(message.Id);
            Assert.False(doesMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.DoesJobIsExistAsync(job.Id);
                Assert.False(doesJobExist);
            }
        }

        [Fact]
        public async Task When_UseTransaction_AddMessageAsync__Message_and_Jobs_ShouldPersistedInDb()
        {
            await using var connection = new MySqlConnection(_mySqlInfraFixture.ConnectionString);
            await connection.OpenAsync();
            Message message;
            IEnumerable<Job> jobs;
            await using (MySqlTransaction? transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                _sut.UseTransaction(transaction);
                (message, jobs) = await _sut.AddMessageAsync("some-payload");
                await transaction.CommitAsync();
            }

            var jobList = jobs.ToList();
            Assert.Equal(2, jobList.Count);

            bool doesMessageExist = await _dbChecks.DoesMessageIsExistAsync(message.Id);
            Assert.True(doesMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.DoesJobIsExistAsync(job.Id);
                Assert.True(doesJobExist);
            }
        }

        [Fact]
        public async Task When_UseTransaction_AddMessageAsync_TransactionNoCommitted__Message_and_Jobs_ShouldNotPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            await using var connection = new MySqlConnection(_mySqlInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                _sut.UseTransaction(transaction);
                (message, jobs) = await _sut.AddMessageAsync("some-payload");
            }

            var jobList = jobs.ToList();

            Assert.NotNull(message);
            Assert.NotEmpty(jobList);

            bool doesMessageExist = await _dbChecks.DoesMessageIsExistAsync(message.Id);
            Assert.False(doesMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.DoesJobIsExistAsync(job.Id);
                Assert.False(doesJobExist);
            }
        }

        private class StringMessageHandler : BaseMessageHandler<string>
        {
            public override Task HandleAsync(string payload, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private class IntMessageHandler : BaseMessageHandler<int>
        {
            public override Task HandleAsync(int payload, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private class ObjMessageHandler : BaseMessageHandler<object>
        {
            public override Task HandleAsync(object payload, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}