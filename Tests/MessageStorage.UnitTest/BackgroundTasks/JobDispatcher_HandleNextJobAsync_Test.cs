using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MessageHandlers;
using Moq;
using Xunit;

namespace MessageStorage.UnitTest.BackgroundTasks
{
    public class JobDispatcher_HandleNextJobAsync_Test
    {
        private readonly JobDispatcher _sut;
        private readonly Mock<IMessageHandlerProvider> _messageHandlerProviderMock;
        private readonly Mock<IJobRepository> _jobRepositoryMock;

        public JobDispatcher_HandleNextJobAsync_Test()
        {
            _messageHandlerProviderMock = new Mock<IMessageHandlerProvider>();
            _jobRepositoryMock = new Mock<IJobRepository>();

            Mock<IRepositoryFactory> repositoryFactoryMock = new Mock<IRepositoryFactory>();
            repositoryFactoryMock.Setup(factory => factory.CreateJobRepository())
                                 .Returns(_jobRepositoryMock.Object);

            _sut = new JobDispatcher(_messageHandlerProviderMock.Object, repositoryFactoryMock.Object);
        }

        [Fact]
        public async Task When_ThereIsNoJobInQueue__ResponseShouldBeFalse()
        {
            _jobRepositoryMock.Setup(repository => repository.SetFirstQueuedJobToInProgressAsync(It.IsAny<CancellationToken>()))
                              .Returns(Task.FromResult(null as Job));

            bool result = await _sut.HandleNextJobAsync();

            Assert.False(result);
        }

        [Fact]
        public async Task When_MessageHandlerThrowsException__JobStatusShouldBeFailed()
        {
            Message message = new Message("payload");
            Job job = new Job(message, "message-handler-type-name");

            _jobRepositoryMock.Setup(repository => repository.SetFirstQueuedJobToInProgressAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(job);

            Mock<IMessageHandler> messageHandlerMock = new Mock<IMessageHandler>();
            messageHandlerMock.Setup(handler => handler.BaseHandleOperationAsync(message.Payload, It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new Exception("some-exception"));
            _messageHandlerProviderMock.Setup(provider => provider.Create(job.MessageHandlerTypeName))
                                       .Returns(messageHandlerMock.Object);

            bool result = await _sut.HandleNextJobAsync();

            Assert.True(result);
            Assert.Equal(JobStatus.Failed, job.JobStatus);
            Assert.NotNull(job.LastOperationInfo);
            Assert.Contains("some-exception", job.LastOperationInfo ?? string.Empty);
        }

        [Fact]
        public async Task When_MessageHandlerExecuted__JobStatusShouldBeCompleted()
        {
            Message message = new Message("payload");
            Job job = new Job(message, "message-handler-type-name");

            _jobRepositoryMock.Setup(repository => repository.SetFirstQueuedJobToInProgressAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(job);

            Mock<IMessageHandler> messageHandlerMock = new Mock<IMessageHandler>();
            messageHandlerMock.Setup(handler => handler.BaseHandleOperationAsync(message.Payload, It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);
            _messageHandlerProviderMock.Setup(provider => provider.Create(job.MessageHandlerTypeName))
                                       .Returns(messageHandlerMock.Object);

            bool result = await _sut.HandleNextJobAsync();

            Assert.True(result);
            Assert.Equal(JobStatus.Completed, job.JobStatus);
        }
    }
}