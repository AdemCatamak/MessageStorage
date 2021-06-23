using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.SqlServer.Extension;
using MessageStorage.SqlServer.IntegrationTest.Checks;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class OutboxClient_AddMessageAsync_Test
    {
        private readonly IMessageStorageClient _sut;
        private readonly SqlServerInfraFixture _sqlServerInfraFixture;
        private const string SCHEMA = "outbox_client_addmessageasync_message_test";

        private readonly DbChecks _dbChecks;

        public OutboxClient_AddMessageAsync_Test(SqlServerInfraFixture sqlServerInfraFixture)
        {
            _sqlServerInfraFixture = sqlServerInfraFixture;

            MessageHandlerContainer messageHandlerContainer = new MessageHandlerContainer();
            messageHandlerContainer.Register<StringMessageHandler>();
            messageHandlerContainer.Register(typeof(IntMessageHandler));
            messageHandlerContainer.Register<ObjMessageHandler>();

            IMessageHandlerProvider messageHandlerProvider = messageHandlerContainer.BuildMessageHandlerProvider();
            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraFixture.ConnectionString, SCHEMA);
            var sqlServerRepositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _dbChecks = new DbChecks(repositoryConfiguration.ConnectionString, repositoryConfiguration.Schema);

            _sut = new MessageStorageClient(messageHandlerProvider, sqlServerRepositoryFactory);
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
        public async Task When_AddMessageAsync__MessageAndJobsShouldPersistedInDb()
        {
            var (message, jobs) = await _sut.AddMessageAsync("some-payload");
            var jobList = jobs.ToList();
            Assert.Equal(2, jobList.Count);

            bool isMessageExist = await _dbChecks.CheckMessageIsExistAsync(message.Id);
            Assert.True(isMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.CheckJobIsExistAsync(job.Id);
                Assert.True(doesJobExist);
            }
        }

        [Fact]
        public async Task When_AddMessageAsync_TransactionNoCommitted__Message_and_Jobs_ShouldNotPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            await using var connection = new SqlConnection(_sqlServerInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                (message, jobs) = await _sut.AddMessageAsync("some-payload", transaction);
            }

            var jobList = jobs.ToList();

            Assert.NotNull(message);
            Assert.NotEmpty(jobList);

            bool doesMessageExist = await _dbChecks.CheckMessageIsExistAsync(message.Id);
            Assert.False(doesMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.CheckJobIsExistAsync(job.Id);
                Assert.False(doesJobExist);
            }
        }


        [Fact]
        public async Task When_UseTransaction_AddMessageAsync__MessageAndJobsShouldPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;

            await using var connection = new SqlConnection(_sqlServerInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                _sut.UseTransaction(transaction.GetMessageStorageTransaction());
                (message, jobs) = await _sut.AddMessageAsync("some-payload");
                await transaction.CommitAsync();
            }

            var jobList = jobs.ToList();
            Assert.Equal(2, jobList.Count);

            bool isMessageExist = await _dbChecks.CheckMessageIsExistAsync(message.Id);
            Assert.True(isMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.CheckJobIsExistAsync(job.Id);
                Assert.True(doesJobExist);
            }
        }

        [Fact]
        public async Task When_UseTransaction_AddMessageAsync_TransactionNoCommitted___Message_and_Jobs_ShouldPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            await using var connection = new SqlConnection(_sqlServerInfraFixture.ConnectionString);
            await connection.OpenAsync();
            await using (DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                _sut.UseTransaction(transaction.GetMessageStorageTransaction());
                (message, jobs) = await _sut.AddMessageAsync("some-payload");
            }

            var jobList = jobs.ToList();

            Assert.NotNull(message);
            Assert.NotEmpty(jobList);

            bool doesMessageExist = await _dbChecks.CheckMessageIsExistAsync(message.Id);
            Assert.False(doesMessageExist);

            foreach (Job job in jobList)
            {
                bool doesJobExist = await _dbChecks.CheckJobIsExistAsync(job.Id);
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