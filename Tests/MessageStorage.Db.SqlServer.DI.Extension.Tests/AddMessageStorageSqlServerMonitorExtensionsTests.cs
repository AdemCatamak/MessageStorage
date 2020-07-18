using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MessageStorage.Db.SqlServer.DI.Extension.Tests
{
    public class AddMessageStorageSqlServerMonitorExtensionsTests
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
        public void WhenAddAddMessageStorageSqlServerMonitor_IMessageStorageMonitorCouldBeResolved()
        {
            _messageStorageServiceCollection.AddMessageStorageSqlServerMonitor(new DbRepositoryConfiguration(string.Empty));

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var messageStorageClient = serviceProvider.GetService<IMessageStorageMonitor>();
                Assert.IsNotNull(messageStorageClient);

                var messageStorageDbClient = serviceProvider.GetService<MessageStorageMonitor>();
                Assert.IsNull(messageStorageDbClient);
            }
        }
    }
}