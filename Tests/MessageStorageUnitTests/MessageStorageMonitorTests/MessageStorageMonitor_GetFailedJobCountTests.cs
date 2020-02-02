using MessageStorage;
using Moq;
using Xunit;

namespace MessageStorageUnitTests.MessageStorageMonitorTests
{
    public class MessageStorageMonitor_GetFailedJobCountTests
    {
        private readonly MessageStorageMonitor _sut;
        private readonly Mock<IStorageAdaptor> _mockStorageAdaptor;

        public MessageStorageMonitor_GetFailedJobCountTests()
        {
            _mockStorageAdaptor = new Mock<IStorageAdaptor>();
            _sut = new MessageStorageMonitor(_mockStorageAdaptor.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-5)]
        public void WhenStorageAdaptorReturnAsWhatever__ResponseShouldBeThoseValues(int storageAdaptorResponse)
        {
            _mockStorageAdaptor.Setup(adaptor => adaptor.GetJobCountByStatus(It.IsAny<JobStatuses>()))
                               .Returns(storageAdaptorResponse);

            int result = _sut.GetJobCountByStatus(It.IsAny<JobStatuses>());

            Assert.Equal(storageAdaptorResponse, result);
        }
    }
}