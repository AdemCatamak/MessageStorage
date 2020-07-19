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
    public class AddMessageStorageSqlServerClientExtensionsTests
    {
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
            _messageStorageServiceCollection.AddMessageStorageSqlServerClient(new DbRepositoryConfiguration(string.Empty), new[] {new Mock<Handler>().Object,});

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                Assert.IsNull(messageStorageClient);

                var messageStorageDbClient = serviceProvider.GetService<IMessageStorageDbClient>();
                Assert.IsNotNull(messageStorageDbClient);

                var messageStorageSqlServerClient = serviceProvider.GetService<SqlServerMessageDbRepository>();
                Assert.IsNull(messageStorageSqlServerClient);
            }
        }

        [Test]
        public void WhenAddAddMessageStorageSqlServerClientWithHandlerManager_IMessageStorageDbClientCouldBeResolved()
        {
            _messageStorageServiceCollection.AddMessageStorageSqlServerClient(new DbRepositoryConfiguration(string.Empty), new Mock<IHandlerManager>().Object);

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetService<IMessageStorageClient>();
                Assert.IsNull(messageStorageClient);

                var messageStorageDbClient = serviceProvider.GetService<IMessageStorageDbClient>();
                Assert.IsNotNull(messageStorageDbClient);

                var messageStorageSqlServerClient = serviceProvider.GetService<SqlServerMessageDbRepository>();
                Assert.IsNull(messageStorageSqlServerClient);
            }
        }
    }
}