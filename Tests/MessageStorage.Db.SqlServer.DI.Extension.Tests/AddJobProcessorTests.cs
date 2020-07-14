using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace MessageStorage.Db.SqlServer.DI.Extension.Tests
{
    public class SqlServerJobProcessorDIExtensionsTests
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
        public void WhenAddSqlServerJobProcessorMethodIsUSedWithHandlerManager__IJobProcessorCouldBeResolved()
        {
            _messageStorageServiceCollection.AddSqlServerJobProcessor(new DummyDbRepositoryConfiguration(), new Mock<IHandlerManager>().Object);
            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var jobProcessor = serviceProvider.GetService<IJobProcessor>();
                Assert.IsNotNull(jobProcessor);

                var jobProcessorBaseDbRepositoryConfig = serviceProvider.GetService<JobProcessor<DbRepositoryConfiguration>>();
                Assert.IsNull(jobProcessorBaseDbRepositoryConfig);

                var jobProcessorDummyDbRepositoryConfig = serviceProvider.GetService<JobProcessor<DummyDbRepositoryConfiguration>>();
                Assert.IsNull(jobProcessorDummyDbRepositoryConfig);
            }
        }

        [Test]
        public void WhenAddSqlServerJobProcessorIsUsed__IJobProcessorCouldBeResolved()
        {
            _messageStorageServiceCollection.AddSqlServerJobProcessor(new DummyDbRepositoryConfiguration(), new[] {new Mock<Handler>().Object});
            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var jobProcessor = serviceProvider.GetService<IJobProcessor>();
                Assert.IsNotNull(jobProcessor);

                var jobProcessorBaseDbRepositoryConfig = serviceProvider.GetService<JobProcessor<DbRepositoryConfiguration>>();
                Assert.IsNull(jobProcessorBaseDbRepositoryConfig);

                var jobProcessorDummyDbRepositoryConfig = serviceProvider.GetService<JobProcessor<DummyDbRepositoryConfiguration>>();
                Assert.IsNull(jobProcessorDummyDbRepositoryConfig);
            }
        }
    }
}