using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.JobProcessServerTests
{
    public class JobProcessServer_StartAsync_Tests
    {
        private JobProcessServer _sut;
        private Mock<ILogger<JobProcessServer>> _mockLogger;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void SetUp()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var jobServerConfiguration = new JobProcessServerConfiguration();
            var mockMessageStorageClient = new Mock<IMessageStorageClient>();
            _mockLogger = new Mock<ILogger<JobProcessServer>>();
            _sut = new JobProcessServer(mockMessageStorageClient.Object, jobServerConfiguration, _mockLogger.Object, _cancellationTokenSource.Token);
        }

        [TearDown]
        public void TearDown()
        {
            _cancellationTokenSource?.Dispose();
            _sut?.Dispose();
        }

        [Test]
        public async Task WhenStartAsyncExecuted__LoggerInfoMethodWillBeExecute()
        {
            await _sut.StartAsync();

            await Task.Delay(10);

            _mockLogger.Verify(logger => logger.Log(LogLevel.Information,
                                                    It.IsAny<EventId>(),
                                                    It.IsAny<Type>(),
                                                    It.IsAny<Exception>(),
                                                    It.IsAny<Func<Type, Exception, string>>()
                                                   ), Times.Once);
        }
    }
}