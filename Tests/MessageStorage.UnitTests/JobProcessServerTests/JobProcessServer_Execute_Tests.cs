using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using MessageStorage.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MessageStorage.UnitTests.JobProcessServerTests
{
    public class JobProcessServer_Execute_Tests : IDisposable
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
            _sut = new JobProcessor(() => _mockRepositoryContext.Object, _mockHandlerManager.Object, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _sut?.Dispose();
        }

        [Test]
        public async Task WhenJobServerExecute__LogDebugWillBeExecuted()
        {
            await _sut.ExecuteAsync();

            _mockLogger.Verify(logger => logger.Log(LogLevel.Debug,
                                                    It.IsAny<EventId>(),
                                                    It.IsAny<Type>(),
                                                    It.IsAny<Exception>(),
                                                    It.IsAny<Func<Type, Exception, string>>()
                                                   ), Times.Exactly(callCount: 2));
        }

        [Test]
        public async Task WhenJobServerExecute_MessageStorageClientReturnNullAsAResult__MessageStorageClientHandleWillNotBeExecute()
        {
            _mockRepositoryContext.Setup(context => context.JobRepository.SetFirstWaitingJobToInProgress())
                                  .Returns(null as Job);

            await _sut.ExecuteAsync();

            _mockHandlerManager.Verify(manager => manager.GetHandler(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task WhenJobServerExecute_MessageStorageClientReturnAJob__MessageStorageClientHandleWillBeExecute()
        {
            _mockRepositoryContext.Setup(context => context.JobRepository.SetFirstWaitingJobToInProgress())
                                  .Returns(new Job("assignedHandler", message: null));

            await _sut.ExecuteAsync();

            _mockHandlerManager.Verify(client => client.GetHandler(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task WhenJobServerExecute_MessageStorageClientReturnAJob_and_MessageStorageClientHandleThrowsException__SetErrorStatusWillBeExecuted()
        {
            _mockRepositoryContext.Setup(context => context.JobRepository.SetFirstWaitingJobToInProgress())
                                  .Returns(new Job("assignedHandler", message: null));

            _mockHandlerManager.Setup(client => client.GetHandler(It.IsAny<string>()))
                               .Throws<NullReferenceException>();

            await _sut.ExecuteAsync();

            _mockRepositoryContext.Verify(client => client.JobRepository.Update(It.Is<Job>(job => job.JobStatus == JobStatuses.Failed)), Times.Once);
        }

        [Test]
        public async Task WhenJobServerExecute_MessageStorageClientReturnAJob_and_MessageStorageClientUpdateThrowsException__LogErrorWillBeExecuted()
        {
            _mockRepositoryContext.Setup(context => context.JobRepository.SetFirstWaitingJobToInProgress())
                                  .Returns(new Job("assignedHandler", message: null));

            _mockRepositoryContext.Setup(client => client.JobRepository.Update(It.IsAny<Job>()))
                                  .Throws<OperationCanceledException>();

            await _sut.ExecuteAsync();

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