using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.JobProcessServerTests
{
    public class JobProcessServer_Execute_Tests : IDisposable
    {
        private JobProcessServer _sut;
        private Mock<ILogger<JobProcessServer>> _mockLogger;
        private CancellationTokenSource _cancellationTokenSource;
        private Mock<IMessageStorageClient> _mockMessageStorageClient;

        [SetUp]
        public void SetUp()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var jobServerConfiguration = new JobProcessServerConfiguration();
            _mockMessageStorageClient = new Mock<IMessageStorageClient>();
            _mockLogger = new Mock<ILogger<JobProcessServer>>();
            _sut = new JobProcessServer(_mockMessageStorageClient.Object, jobServerConfiguration, _mockLogger.Object, _cancellationTokenSource.Token);
        }

        [TearDown]
        public void TearDown()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        [Test]
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

        [Test]
        public void WhenJobServerExecute_MessageStorageClientReturnNullAsAResult__MessageStorageClientHandleWillNotBeExecute()
        {
            _mockMessageStorageClient.Setup(client => client.SetFirstWaitingJobToInProgress())
                                     .Returns(null as Job);

            _sut.Execute();

            _mockMessageStorageClient.Verify(client => client.Handle(It.IsAny<Job>()), Times.Never);
        }

        [Test]
        public void WhenJobServerExecute_MessageStorageClientReturnAJob__MessageStorageClientHandleWillBeExecute()
        {
            _mockMessageStorageClient.Setup(client => client.SetFirstWaitingJobToInProgress())
                                     .Returns(new Job(null, "assignedHandler"));

            _sut.Execute();

            _mockMessageStorageClient.Verify(client => client.Handle(It.IsAny<Job>()), Times.Once);
        }

        [Test]
        public void WhenJobServerExecute_MessageStorageClientReturnAJob_and_MessageStorageClientHandleThrowsException__SetErrorStatusWillBeExecuted()
        {
            _mockMessageStorageClient.Setup(client => client.SetFirstWaitingJobToInProgress())
                                     .Returns(new Job(null, "assignedHandler"));

            _mockMessageStorageClient.Setup(client => client.Handle(It.IsAny<Job>()))
                                     .Throws<NullReferenceException>();

            _sut.Execute();

            _mockMessageStorageClient.Verify(client => client.Handle(It.IsAny<Job>()), Times.Once);
            _mockMessageStorageClient.Verify(client => client.Update(It.Is<Job>(job => job.JobStatus == JobStatuses.Failed)), Times.Once);
        }

        [Test]
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