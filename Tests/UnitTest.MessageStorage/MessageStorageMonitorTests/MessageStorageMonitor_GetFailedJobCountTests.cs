using MessageStorage;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.MessageStorageMonitorTests
{
    public class MessageStorageMonitor_GetFailedJobCountTests
    {
        private MessageStorageMonitor _sut;
        private Mock<IJobRepository> _mockJobRepository;

        [SetUp]
        public void SetUp()
        {
            _mockJobRepository = new Mock<IJobRepository>();

            var mockRepositoryManager = new Mock<IRepositoryResolver>();
            mockRepositoryManager.Setup(manager => manager.Resolve<IJobRepository>())
                                 .Returns(_mockJobRepository.Object);

            _sut = new MessageStorageMonitor(mockRepositoryManager.Object);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsXForFailedJobCount__ResponseShouldBeThoseValues([Values(0, 1, -5)] int storageAdaptorResponse)
        {
            _mockJobRepository.Setup(adaptor => adaptor.GetJobCountByStatus(JobStatuses.Failed))
                              .Returns(storageAdaptorResponse);

            int result = _sut.GetFailedJobCount();

            Assert.AreEqual(storageAdaptorResponse, result);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsZeroForFailedJob_ResponseShouldBeThoseValues([Values(0, 1, -5)] int storageAdaptorResponse)
        {
            _mockJobRepository.Setup(adaptor => adaptor.GetJobCountByStatus(It.IsAny<JobStatuses>()))
                              .Returns(storageAdaptorResponse);

            _mockJobRepository.Setup(adaptor => adaptor.GetJobCountByStatus(JobStatuses.Failed))
                              .Returns(value: 0);

            int result = _sut.GetFailedJobCount();

            Assert.AreEqual(expected: 0, result);
        }
    }
}