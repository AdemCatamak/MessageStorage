using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MySql.Extension;
using MessageStorage.MySql.FunctionalTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using TestUtility;
using Xunit;

namespace MessageStorage.MySql.FunctionalTest
{
    [Collection(TestServerFixture.FIXTURE_KEY)]
    public class OutboxTransactionNotCommitTest : IDisposable
    {
        private readonly TestServerFixture _testServerFixture;

        private readonly RepositoryConfiguration _repositoryConfiguration;

        private readonly IServiceScope _serviceScope;
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly ShipmentDbContext _shipmentDbContext;

        public OutboxTransactionNotCommitTest(TestServerFixture testServerFixture)
        {
            _testServerFixture = testServerFixture;

            _serviceScope = _testServerFixture.GetServiceScope();
            IServiceProvider serviceProvider = _serviceScope.ServiceProvider;
            _messageStorageClient = serviceProvider.GetRequiredService<IMessageStorageClient>();
            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            _repositoryConfiguration = repositoryFactory.RepositoryConfiguration;

            _shipmentDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);

            RunMigration(_repositoryConfiguration);
        }

        private void RunMigration(RepositoryConfiguration repositoryConfiguration)
        {
            using var connection = new MySqlConnection(repositoryConfiguration.ConnectionString);
            connection.Execute(@$"
CREATE SCHEMA IF NOT EXISTS not_committed;

CREATE TABLE IF NOT EXISTS not_committed.shipments (
    id serial,
    buyer_name varchar(500) NOT NULL,
    created_on timestamp NOT NULL,
    PRIMARY KEY (id)
);
");
        }


        [Fact]
        public async Task WhenTransactionNotCommitted__JobIsNotPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            ShipmentEntity shipmentEntity;
            await using (MySqlTransaction transaction = (MySqlTransaction) (await _shipmentDbContext.Database.BeginTransactionAsync()).GetDbTransaction())
            {
                shipmentEntity = new ShipmentEntity("buyer-name");
                _shipmentDbContext.ShipmentEntities.Add(shipmentEntity);
                await _shipmentDbContext.SaveChangesAsync();

                AssertThat.GreaterThan(0, shipmentEntity.Id);

                ShipmentCreated shipmentCreated = new ShipmentCreated(shipmentEntity.Id, shipmentEntity.BuyerName, shipmentEntity.CreatedOn);
                IMessageStorageTransaction messageStorageTransaction = transaction.GetMessageStorageTransaction();
                (message, jobs) = await _messageStorageClient.AddMessageAsync(shipmentCreated, messageStorageTransaction);
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstOrDefaultAsync(messageScript, new {id = message.Id});
            Assert.Null(messageIdResult);


            string script = $"select id as job_id ,message_id from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstOrDefaultAsync(script, new {id = job.Id});
            Assert.Null(jobMessageResult);

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.Null(fetchedShipmentEntity);
        }

        [Fact(Skip = "PostgreSQL doesn't support nested or concurrent transactions")]
        public async Task WhenTransactionScopeNotCommitted__JobIsNotPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            ShipmentEntity shipmentEntity;
            using (TransactionScope _ = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                shipmentEntity = new ShipmentEntity("buyer-name");
                _shipmentDbContext.ShipmentEntities.Add(shipmentEntity);
                await _shipmentDbContext.SaveChangesAsync();

                AssertThat.GreaterThan(0, shipmentEntity.Id);

                ShipmentCreated shipmentCreated = new ShipmentCreated(shipmentEntity.Id, shipmentEntity.BuyerName, shipmentEntity.CreatedOn);
                (message, jobs) = await _messageStorageClient.AddMessageAsync(shipmentCreated);
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.Null(fetchedShipmentEntity);

            await using var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstOrDefaultAsync(messageScript, new {id = message.Id});
            Assert.Null(messageIdResult);


            string script = $"select id as job_id ,message_id from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstOrDefaultAsync(script, new {id = job.Id});
            Assert.Null(jobMessageResult);
        }

        [Fact]
        public async Task WhenDbTransactionNotCommitted__JobIsNotPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            ShipmentEntity shipmentEntity;
            await using (DbTransaction transaction = (await _shipmentDbContext.Database.BeginTransactionAsync()).GetDbTransaction())
            {
                shipmentEntity = new ShipmentEntity("buyer-name");
                _shipmentDbContext.ShipmentEntities.Add(shipmentEntity);
                await _shipmentDbContext.SaveChangesAsync();

                AssertThat.GreaterThan(0, shipmentEntity.Id);

                ShipmentCreated shipmentCreated = new ShipmentCreated(shipmentEntity.Id, shipmentEntity.BuyerName, shipmentEntity.CreatedOn);
                (message, jobs) = await _messageStorageClient.AddMessageAsync(shipmentCreated, transaction);
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstOrDefaultAsync(messageScript, new {id = message.Id});
            Assert.Null(messageIdResult);


            string script = $"select id as job_id ,message_id from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstOrDefaultAsync(script, new {id = job.Id});
            Assert.Null(jobMessageResult);

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.Null(fetchedShipmentEntity);
        }

        [Fact]
        public async Task WhenNpgsqlTransactionNotCommitted__JobIsNotPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            ShipmentEntity shipmentEntity;
            await using (MySqlTransaction transaction = (MySqlTransaction) (await _shipmentDbContext.Database.BeginTransactionAsync()).GetDbTransaction())
            {
                shipmentEntity = new ShipmentEntity("buyer-name");
                _shipmentDbContext.ShipmentEntities.Add(shipmentEntity);
                await _shipmentDbContext.SaveChangesAsync();

                AssertThat.GreaterThan(0, shipmentEntity.Id);

                ShipmentCreated shipmentCreated = new ShipmentCreated(shipmentEntity.Id, shipmentEntity.BuyerName, shipmentEntity.CreatedOn);
                (message, jobs) = await _messageStorageClient.AddMessageAsync(shipmentCreated, transaction);
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstOrDefaultAsync(messageScript, new {id = message.Id});
            Assert.Null(messageIdResult);


            string script = $"select id as job_id ,message_id from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstOrDefaultAsync(script, new {id = job.Id});
            Assert.Null(jobMessageResult);

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.Null(fetchedShipmentEntity);
        }

        public class ShipmentEntity
        {
            public int Id { get; private set; } = 0;
            public string BuyerName { get; private set; }
            public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

            public ShipmentEntity(string buyerName)
            {
                BuyerName = buyerName;
            }
        }

        public class ShipmentDbContext : DbContext
        {
            public ShipmentDbContext(string connectionStr) :
                base(new DbContextOptionsBuilder().UseMySql(connectionStr, new MySqlServerVersion("5")).Options)
            {
            }

            public DbSet<ShipmentEntity> ShipmentEntities { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ShipmentEntity>()
                            .ToTable("shipments", "not_committed")
                            .HasKey(x => x.Id);
                modelBuilder.Entity<ShipmentEntity>()
                            .Property(x => x.Id)
                            .HasColumnName("id");
                modelBuilder.Entity<ShipmentEntity>()
                            .Property(x => x.BuyerName)
                            .HasColumnName("buyer_name");
                modelBuilder.Entity<ShipmentEntity>()
                            .Property(x => x.CreatedOn)
                            .HasColumnName("created_on");

                base.OnModelCreating(modelBuilder);
            }
        }

        public class ShipmentCreated
        {
            public int ShipmentId { get; private set; }
            public string BuyerName { get; private set; }
            public DateTime ShipmentCreatedOn { get; private set; }

            public ShipmentCreated(int shipmentId, string buyerName, DateTime shipmentCreatedOn)
            {
                ShipmentId = shipmentId;
                BuyerName = buyerName;
                ShipmentCreatedOn = shipmentCreatedOn;
            }
        }

        public class ShipmentCreated_SendEmail : BaseMessageHandler<ShipmentCreated>
        {
            public override Task HandleAsync(ShipmentCreated payload, CancellationToken cancellationToken = default)
            {
                if (payload.ShipmentId <= 1) throw new ValidationException("Shipment Id should not be empty");
                if (string.IsNullOrEmpty(payload.BuyerName)) throw new ValidationException("Buyer Name should not be empty");
                if (payload.ShipmentCreatedOn != default) throw new ValidationException("Shipment Created On should not be empty");

                return Task.CompletedTask;
            }
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
            _shipmentDbContext.Dispose();
        }
    }
}