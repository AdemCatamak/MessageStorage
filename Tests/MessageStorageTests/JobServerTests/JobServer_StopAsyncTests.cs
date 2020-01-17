using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MessageStorageTests.JobServerTests
{
    public class JobServer_StopAsyncTests : IDisposable
    {
        private readonly JobServer _sut;
        private readonly Mock<ILogger<JobServer>> _mockLogger;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public JobServer_StopAsyncTests()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var jobServerConfiguration = new JobServerConfiguration();
            var mockMessageStorageClient = new Mock<IMessageStorageClient>();
            _mockLogger = new Mock<ILogger<JobServer>>();
            _sut = new JobServer(mockMessageStorageClient.Object, jobServerConfiguration, _mockLogger.Object, _cancellationTokenSource.Token);
        }

        [Fact]
        public async Task WhenStopAsyncExecuted__LoggerInfoMethodWillBeExecute()
        {
            await _sut.StopAsync();

            await Task.Delay(10);

            _mockLogger.Verify(logger => logger.Log(LogLevel.Information,
                                                    It.IsAny<EventId>(),
                                                    It.IsAny<Type>(),
                                                    It.IsAny<Exception>(),
                                                    It.IsAny<Func<Type, Exception, string>>()
                                                   ), Times.Once);
        }


        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            _sut?.Dispose();
        }
    }
}