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
using MessageStorage.SqlServer.Extension;
using MessageStorage.SqlServer.FunctionalTest.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.FunctionalTest
{
    [Collection(TestServerFixture.FIXTURE_KEY)]
    public class OutboxTransactionCommitTest : IDisposable
    {
        private readonly TestServerFixture _testServerFixture;

        private readonly RepositoryConfiguration _repositoryConfiguration;

        private readonly IServiceScope _serviceScope;
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly ShipmentDbContext _shipmentDbContext;

        public OutboxTransactionCommitTest(TestServerFixture testServerFixture)
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
            string script = @"
if not exists (select * from sys.tables t join sys.schemas s on (t.schema_id = s.schema_id) where s.name = 'dbo' and t.name = 'shipments')
    create table dbo.shipments (
        id int identity(1,1) not null primary key,
        buyer_name nvarchar(500) not null,
        created_on datetime not null
    );
";
            using var connection = new SqlConnection(repositoryConfiguration.ConnectionString);
            connection.Execute(script);
        }

        [Fact]
        public async Task When_ForgettyTransactionCommitted__JobIsPersistedInDb()
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
                IMessageStorageTransaction messageStorageTransaction = transaction.GetMessageStorageTransaction();
                (message, jobs) = await _messageStorageClient.AddMessageAsync(shipmentCreated, messageStorageTransaction);

                await transaction.CommitAsync();
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using var connection = new SqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstAsync(messageScript, new {id = message.Id});
            var messageIdStr = ((Guid) messageIdResult.id).ToString();
            Assert.NotEmpty(messageIdStr);

            string script = $"select Id as JobId ,MessageId from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstAsync(script, new {id = job.Id});
            Assert.Equal(message.Id, jobMessageResult.MessageId);
            Assert.Equal(job.Id, jobMessageResult.JobId);

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.NotNull(fetchedShipmentEntity);
            AssertThat.GreaterThan(0, fetchedShipmentEntity?.Id ?? 0);
        }

        [Fact]
        public async Task When_TransactionScopeCommitted__JobIsPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            ShipmentEntity shipmentEntity;
            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                shipmentEntity = new ShipmentEntity("buyer-name");
                _shipmentDbContext.ShipmentEntities.Add(shipmentEntity);
                await _shipmentDbContext.SaveChangesAsync();

                AssertThat.GreaterThan(0, shipmentEntity.Id);

                ShipmentCreated shipmentCreated = new ShipmentCreated(shipmentEntity.Id, shipmentEntity.BuyerName, shipmentEntity.CreatedOn);
                (message, jobs) = await _messageStorageClient.AddMessageAsync(shipmentCreated);

                transaction.Complete();
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using var connection = new SqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstAsync(messageScript, new {id = message.Id});
            var messageIdStr = ((Guid) messageIdResult.id).ToString();
            Assert.NotEmpty(messageIdStr);

            string script = $"select Id as JobId ,MessageId from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstAsync(script, new {id = job.Id});
            Assert.Equal(message.Id, jobMessageResult.MessageId);
            Assert.Equal(job.Id, jobMessageResult.JobId);

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.NotNull(fetchedShipmentEntity);
            AssertThat.GreaterThan(0, fetchedShipmentEntity?.Id ?? 0);
        }

        [Fact]
        public async Task When_TransactionCommitted_with_OutboxExtension__JobIsPersistedInDb()
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

                await transaction.CommitAsync();
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using var connection = new SqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstAsync(messageScript, new {id = message.Id});
            var messageIdStr = ((Guid) messageIdResult.id).ToString();
            Assert.NotEmpty(messageIdStr);

            string script = $"select Id as JobId ,MessageId from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstAsync(script, new {id = job.Id});
            Assert.Equal(message.Id, jobMessageResult.MessageId);
            Assert.Equal(job.Id, jobMessageResult.JobId);

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.NotNull(fetchedShipmentEntity);
            AssertThat.GreaterThan(0, fetchedShipmentEntity?.Id ?? 0);
        }

        [Fact]
        public async Task WhenSqlTransactionCommitted_with_OutboxExtension__JobIsPersistedInDb()
        {
            Message message;
            IEnumerable<Job> jobs;
            ShipmentEntity shipmentEntity;
            await using (SqlTransaction transaction = (SqlTransaction) (await _shipmentDbContext.Database.BeginTransactionAsync()).GetDbTransaction())
            {
                shipmentEntity = new ShipmentEntity("buyer-name");
                _shipmentDbContext.ShipmentEntities.Add(shipmentEntity);
                await _shipmentDbContext.SaveChangesAsync();

                AssertThat.GreaterThan(0, shipmentEntity.Id);

                ShipmentCreated shipmentCreated = new ShipmentCreated(shipmentEntity.Id, shipmentEntity.BuyerName, shipmentEntity.CreatedOn);
                (message, jobs) = await _messageStorageClient.AddMessageAsync(shipmentCreated, transaction);

                await transaction.CommitAsync();
            }

            var jobList = jobs.ToList();
            Assert.Single(jobList);
            Job job = jobList.First();

            await using var connection = new SqlConnection(_repositoryConfiguration.ConnectionString);

            string messageScript = $"select id from {_repositoryConfiguration.Schema}.messages where id = @id";
            dynamic? messageIdResult = await connection.QueryFirstAsync(messageScript, new {id = message.Id});
            var messageIdStr = ((Guid) messageIdResult.id).ToString();
            Assert.NotEmpty(messageIdStr);

            string script = $"select Id as JobId ,MessageId from {_repositoryConfiguration.Schema}.jobs where id = @id";
            dynamic? jobMessageResult = await connection.QueryFirstAsync(script, new {id = job.Id});
            Assert.Equal(message.Id, jobMessageResult.MessageId);
            Assert.Equal(job.Id, jobMessageResult.JobId);

            await using ShipmentDbContext checkDbContext = new ShipmentDbContext(_repositoryConfiguration.ConnectionString);
            ShipmentEntity? fetchedShipmentEntity = checkDbContext.ShipmentEntities.FirstOrDefault(s => s.Id == shipmentEntity.Id);
            Assert.NotNull(fetchedShipmentEntity);
            AssertThat.GreaterThan(0, fetchedShipmentEntity?.Id ?? 0);
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
            public ShipmentDbContext(string connectionStr)
                : base(new DbContextOptionsBuilder().UseSqlServer(connectionStr).Options)
            {
            }

            public DbSet<ShipmentEntity> ShipmentEntities { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ShipmentEntity>()
                            .ToTable("shipments")
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
            _shipmentDbContext?.Dispose();
            _serviceScope.Dispose();
        }
    }
}