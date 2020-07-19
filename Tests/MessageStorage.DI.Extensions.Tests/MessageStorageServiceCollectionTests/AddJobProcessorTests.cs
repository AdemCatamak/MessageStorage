using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MessageStorage.DI.Extensions.Tests.MessageStorageServiceCollectionTests
{
    public class AddJobProcessorTests
    {
        private MessageStorageServiceCollection _sut;
        private ServiceCollection _serviceCollection;
        private Mock<IHandlerManager> _mockHandlerManager;
        private Mock<IRepositoryContext> _mockRepositoryContext;
        private Mock<ILogger<IJobProcessor>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockHandlerManager = new Mock<IHandlerManager>();
            _mockRepositoryContext = new Mock<IRepositoryContext>();
            _mockLogger = new Mock<ILogger<IJobProcessor>>();

            _serviceCollection = new ServiceCollection();
            _sut = new MessageStorageServiceCollection(_serviceCollection);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void WhenAddRepositoryContext_WithoutType__DerivedTypeShouldBeInjected()
        {
            _sut.AddJobProcessor(provider => new JobProcessor(()=>_mockRepositoryContext.Object, _mockHandlerManager.Object, _mockLogger.Object));

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var jobProcessor = serviceProvider.GetService<JobProcessor>();
                Assert.NotNull(jobProcessor);

                var baseJobProcessor = serviceProvider.GetService<IJobProcessor>();
                Assert.Null(baseJobProcessor);
            }
        }

        [Test]
        public void WhenAddMessageStorageClient_WithType__DefinedTypeShouldBeInjected()
        {
            _sut.AddJobProcessor<IJobProcessor>(provider => new JobProcessor(()=>_mockRepositoryContext.Object, _mockHandlerManager.Object, _mockLogger.Object));

            using (ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                var jobProcessor = serviceProvider.GetService<JobProcessor>();
                Assert.Null(jobProcessor);

                var baseJobProcessor = serviceProvider.GetService<IJobProcessor>();
                Assert.NotNull(baseJobProcessor);
            }
        }
    }
}