using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MessageStorage.UnitTests.JobProcessServerTests
{
    public class JobProcessServer_StartAsync_Tests
    {
        private JobProcessor _sut;
        private Mock<ILogger<IJobProcessor>> _mockLogger;
        private CancellationTokenSource _cancellationTokenSource;
        private Mock<IRepositoryContext> _mockRepositoryContext;
        private Mock<IHandlerManager> _mockHandlerManager;

        [SetUp]
        public void SetUp()
        {
            _mockHandlerManager = new Mock<IHandlerManager>();
            _cancellationTokenSource = new CancellationTokenSource();
            _mockRepositoryContext = new Mock<IRepositoryContext>();
            _mockLogger = new Mock<ILogger<IJobProcessor>>();
            _sut = new JobProcessor(()=>_mockRepositoryContext.Object, _mockHandlerManager.Object, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _sut?.Dispose();
        }

        [Test]
        public async Task WhenStartAsyncExecuted__LoggerInfoMethodWillBeExecute()
        {
            await _sut.StartAsync(_cancellationTokenSource.Token);
            
            await Task.Delay(millisecondsDelay: 10);

            _mockLogger.Verify(logger => logger.Log(LogLevel.Debug,
                                                    It.IsAny<EventId>(),
                                                    It.IsAny<Type>(),
                                                    It.IsAny<Exception>(),
                                                    It.IsAny<Func<Type, Exception, string>>()
                                                   ), Times.AtLeast(callCount: 2));
        }
    }
}