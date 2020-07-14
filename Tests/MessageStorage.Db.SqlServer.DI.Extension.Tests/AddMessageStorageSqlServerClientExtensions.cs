using MessageStorage.Clients;
using MessageStorage.Db.Clients;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.SqlServer.DataAccessSection.Repositories;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace MessageStorage.Db.SqlServer.DI.Extension.Tests
{
    public class AddMessageStorageSqlServerClientExtensions
    {
        private class DummyDbRepositoryConfiguration : DbRepositoryConfiguration
        {
        }

        private IServiceCollection _serviceCollection;
        private MessageStorageServiceCollection _messageStorageServiceCollection;

        [SetUp]
        public void Setup()
        {
            _serviceCollection = new ServiceCollection();
            _messageStorageServiceCollection = new MessageStorageServiceCollection(_serviceCollection);
        }

        [Test]
        public void WhenAddAddMessageStorageSqlServerClientWithHandlerList_IMessageStorageDbClientCouldBeResolved()
        {
            _messageStorageServiceCollection.AddMessageStorageSqlServerClient(new DummyDbRepositoryConfiguration(), new[] {new Mock<Handler>().Object,});

            using (var serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                Assert.IsNull(messageStorageClient);

                var messageStorageDbClient = serviceProvider.GetService<IMessageStorageDbClient>();
                Assert.IsNotNull(messageStorageDbClient);

                var messageStorageSqlServerClientWithBaseConfig = serviceProvider.GetService<SqlServerDbMessageRepository<DbRepositoryConfiguration>>();
                Assert.IsNotNull(messageStorageSqlServerClientWithBaseConfig);

                var messageStorageSqlServerClientWithDerivedConfig = serviceProvider.GetService<SqlServerDbMessageRepository<DummyDbRepositoryConfiguration>>();
                Assert.IsNotNull(messageStorageSqlServerClientWithDerivedConfig);
            }
        }

        [Test]
        public void WhenAddAddMessageStorageSqlServerClientWithHandlerManager_IMessageStorageDbClientCouldBeResolved()
        {
            _messageStorageServiceCollection.AddMessageStorageSqlServerClient(new DummyDbRepositoryConfiguration(), new Mock<IHandlerManager>().Object);

            using (var serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                Assert.IsNull(messageStorageClient);

                var messageStorageDbClient = serviceProvider.GetService<IMessageStorageDbClient>();
                Assert.IsNotNull(messageStorageDbClient);

                var messageStorageSqlServerClientWithBaseConfig = serviceProvider.GetService<SqlServerDbMessageRepository<DbRepositoryConfiguration>>();
                Assert.IsNotNull(messageStorageSqlServerClientWithBaseConfig);

                var messageStorageSqlServerClientWithDerivedConfig = serviceProvider.GetService<SqlServerDbMessageRepository<DummyDbRepositoryConfiguration>>();
                Assert.IsNotNull(messageStorageSqlServerClientWithDerivedConfig);
            }
        }
    }
}