using MessageStorage;
using Moq;
using Xunit;

namespace MessageStorageTests.MessageStorageClientTests
{
    public class MessageStorageClient_SetFirstWaitingJobToInProgressTests
    {
        private readonly Mock<IStorageAdaptor> _mockStorageAdaptor;

        private readonly MessageStorageClient _sut;

        public MessageStorageClient_SetFirstWaitingJobToInProgressTests()
        {
            _mockStorageAdaptor = new Mock<IStorageAdaptor>();
            var mockHandlerManager = new Mock<IHandlerManager>();

            _sut = new MessageStorageClient(_mockStorageAdaptor.Object, mockHandlerManager.Object);
        }

        [Fact]
        public void WhenSetFirstWaitingJobToInProgressExecuted__IStorageAdaptorWillBeExecuted()
        {
            _sut.SetFirstWaitingJobToInProgress();

            _mockStorageAdaptor.Verify(adaptor => adaptor.SetFirstWaitingJobToInProgress(), Times.Once);
        }
    }
}