using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MessageStorageTests.JobProcessServerTests
{
    public class JobProcessServer_ExecuteTests : IDisposable
    {
        private readonly JobProcessServer _sut;
        private readonly Mock<ILogger<JobProcessServer>> _mockLogger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Mock<IMessageStorageClient> _mockMessageStorageClient;

        public JobProcessServer_ExecuteTests()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var jobServerConfiguration = new JobProcessServerConfiguration();
            _mockMessageStorageClient = new Mock<IMessageStorageClient>();
            _mockLogger = new Mock<ILogger<JobProcessServer>>();
            _sut = new JobProcessServer(_mockMessageStorageClient.Object, jobServerConfiguration, _mockLogger.Object, _cancellationTokenSource.Token);
        }

        [Fact]
        public void WhenJobServerExecute__LogDebugWillBeExecuted()
        {
            _sut.Execute();

            _mockLogger.Verify(logger => logger.Log(LogLevel.Debug,
                                                    It.IsAny<EventId>(),
                                                    It.IsAny<Type>(),
                                                    It.IsAny<Exception>(),
                                                    It.IsAny<Func<Type, Exception, string>>()
                                                   ), Times.Once);
        }

        [Fact]
        public void WhenJobServerExecute_MessageStorageClientReturnNullAsAResult__MessageStorageClientHandleWillNotBeExecute()
        {
            _mockMessageStorageClient.Setup(client => client.SetFirstWaitingJobToInProgress())
                                     .Returns(null as Job);

            _sut.Execute();

            _mockMessageStorageClient.Verify(client => client.Handle(It.IsAny<Job>()), Times.Never);
        }

        [Fact]
        public void WhenJobServerExecute_MessageStorageClientReturnAJob__MessageStorageClientHandleWillBeExecute()
        {
            _mockMessageStorageClient.Setup(client => client.SetFirstWaitingJobToInProgress())
                                     .Returns(new Job(null, "assignedHandler"));

            _sut.Execute();

            _mockMessageStorageClient.Verify(client => client.Handle(It.IsAny<Job>()), Times.Once);
        }

        [Fact]
        public void WhenJobServerExecute_MessageStorageClientReturnAJob_and_MessageStorageClientUpdateThrowsException__LogErrorWillBeExecuted()
        {
            _mockMessageStorageClient.Setup(client => client.SetFirstWaitingJobToInProgress())
                                     .Returns(new Job(null, "assignedHandler"));

            _mockMessageStorageClient.Setup(client => client.Handle(It.IsAny<Job>()))
                                     .Returns(() => Task.CompletedTask);

            _mockMessageStorageClient.Setup(client => client.Update(It.IsAny<Job>()))
                                     .Throws<OperationCanceledException>();

            _sut.Execute();

            _mockMessageStorageClient.Verify(client => client.Handle(It.IsAny<Job>()), Times.Once);
            _mockMessageStorageClient.Verify(client => client.Update(It.IsAny<Job>()), Times.Once);
            _mockLogger.Verify(logger => logger.Log(LogLevel.Error,
                                                    It.IsAny<EventId>(),
                                                    It.IsAny<Type>(),
                                                    It.IsAny<Exception>(),
                                                    It.IsAny<Func<Type, Exception, string>>()
                                                   ), Times.Once);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}