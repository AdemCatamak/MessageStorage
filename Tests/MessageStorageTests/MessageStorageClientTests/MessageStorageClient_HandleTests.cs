using System;
using System.Threading.Tasks;
using MessageStorage;
using MessageStorage.Exceptions;
using Moq;
using Xunit;

namespace MessageStorageTests.MessageStorageClientTests
{
    public class MessageStorageClient_HandleTests
    {
        private readonly Mock<IStorageAdaptor> _mockStorageAdaptor;
        private readonly Mock<IHandlerManager> _mockHandlerManager;

        private readonly MessageStorageClient _sut;

        public MessageStorageClient_HandleTests()
        {
            _mockStorageAdaptor = new Mock<IStorageAdaptor>();
            _mockHandlerManager = new Mock<IHandlerManager>();

            _sut = new MessageStorageClient(_mockStorageAdaptor.Object, _mockHandlerManager.Object);
        }

        [Fact]
        public async Task WhenHandleExecuted_HandlerManagerGetHandleReturnNull__NullReferenceOccurs()
        {
            _mockHandlerManager.Setup(manager => manager.GetHandler(It.IsAny<string>()))
                               .Returns(null as Handler);

            await Assert.ThrowsAsync<HandlerNotFoundException>(async () => await _sut.Handle(It.IsAny<Job>()));
        }

        [Fact]
        public async Task WhenHandleExecuted_and_MessageIsNull__NullReferenceExceptionOccurs()
        {
            var mockHandler = new Mock<Handler>();
            _mockHandlerManager.Setup(manager => manager.GetHandler(It.IsAny<string>()))
                               .Returns(mockHandler.Object);

            var job = new Job(null, "assignedHandlerName");
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _sut.Handle(job));
        }

        [Fact]
        public async Task WhenHandleExecuted_HandlerManagerGetHandleReturnHandler__HandlerHandleMethodWillBeExecuted()
        {
            var mockHandler = new Mock<Handler>();
            _mockHandlerManager.Setup(manager => manager.GetHandler(It.IsAny<string>()))
                               .Returns(mockHandler.Object);

            var job = new Job(new Message(null), "assignedHandlerName");
            await _sut.Handle(job);

            mockHandler.Verify(handler => handler.Handle(It.IsAny<Job>()));
        }
    }
}