using MessageStorage;
using Moq;
using Xunit;

namespace MessageStorageTests.MessageStorageClientTests
{
    public class MessageStorageClient_UpdateTests
    {
        private readonly Mock<IStorageAdaptor> _mockStorageAdaptor;

        private readonly MessageStorageClient _sut;

        public MessageStorageClient_UpdateTests()
        {
            _mockStorageAdaptor = new Mock<IStorageAdaptor>();
            var mockHandlerManager = new Mock<IHandlerManager>();

            _sut = new MessageStorageClient(_mockStorageAdaptor.Object, mockHandlerManager.Object);
        }

        [Fact]
        public void WhenUpdateExecuted__IStorageAdaptorWillBeExecuted()
        {
            _sut.Update(default);

            _mockStorageAdaptor.Verify(adaptor => adaptor.Update(It.IsAny<Job>()), Times.Once);
        }
    }
}