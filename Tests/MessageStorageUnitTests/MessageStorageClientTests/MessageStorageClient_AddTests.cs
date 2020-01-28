using System.Collections.Generic;
using MessageStorage;
using Moq;
using Xunit;

namespace MessageStorageUnitTests.MessageStorageClientTests
{
    public class MessageStorageClient_AddTests
    {
        private readonly Mock<IStorageAdaptor> _mockStorageAdaptor;
        private readonly Mock<IHandlerManager> _mockHandlerManager;

        private readonly MessageStorageClient _sut;

        public MessageStorageClient_AddTests()
        {
            _mockStorageAdaptor = new Mock<IStorageAdaptor>();
            _mockHandlerManager = new Mock<IHandlerManager>();

            _sut = new MessageStorageClient(_mockStorageAdaptor.Object, _mockHandlerManager.Object);
        }

        [Fact]
        public void WhenAddExecuted_and_JobCreatorIsTrue__HandlerManagerGetAvailableHandler_and_StorageAdaptorAdd_WillBeExecuted()
        {
            var payload = new object();
            _sut.Add(payload, autoJobCreator: true);

            _mockHandlerManager.Verify(manager => manager.GetAvailableHandlerNames(payload), Times.Once);
            _mockStorageAdaptor.Verify(adaptor => adaptor.Add(It.IsAny<Message>(), It.IsAny<IEnumerable<Job>>()), Times.Once);
        }

        [Fact]
        public void WhenAddExecuted_and_JobCreatorIsFalse__HandlerManagerGetAvailableHandlerWillNotExecute_and_StorageAdaptorAddWillBeExecuted()
        {
            var payload = new object();
            _sut.Add(payload, autoJobCreator: false);

            _mockHandlerManager.Verify(manager => manager.GetAvailableHandlerNames(It.IsAny<object>()), Times.Never);
            _mockStorageAdaptor.Verify(adaptor => adaptor.Add(It.IsAny<Message>(), It.IsAny<IEnumerable<Job>>()), Times.Once);
        }
    }
}