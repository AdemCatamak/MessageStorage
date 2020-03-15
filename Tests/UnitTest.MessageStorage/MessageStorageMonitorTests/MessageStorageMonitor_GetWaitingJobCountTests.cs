using MessageStorage;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.MessageStorageMonitorTests
{
    public class MessageStorageMonitor_GetWaitingJobCountTests
    {
        private readonly MessageStorageMonitor _sut;
        private readonly Mock<IJobRepository> _mockJobRepository;

        public MessageStorageMonitor_GetWaitingJobCountTests()
        {
            _mockJobRepository = new Mock<IJobRepository>();

            var mockRepositoryManager = new Mock<IRepositoryResolver>();
            mockRepositoryManager.Setup(manager => manager.Resolve<IJobRepository>())
                                 .Returns(_mockJobRepository.Object);

            _sut = new MessageStorageMonitor(mockRepositoryManager.Object);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsXForWaitingJobCount__ResponseShouldBeThoseValues([Values(0, 1, -5)] int storageAdaptorResponse)
        {
            _mockJobRepository.Setup(adaptor => adaptor.GetJobCountByStatus(JobStatuses.Waiting))
                              .Returns(storageAdaptorResponse);

            int result = _sut.GetWaitingJobCount();

            Assert.AreEqual(storageAdaptorResponse, result);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsZero_ResponseShouldBeThoseValues([Values(0, 1, -5)] int storageAdaptorResponse)
        {
            _mockJobRepository.Setup(adaptor => adaptor.GetJobCountByStatus(It.IsAny<JobStatuses>()))
                              .Returns(storageAdaptorResponse);

            _mockJobRepository.Setup(adaptor => adaptor.GetJobCountByStatus(JobStatuses.Waiting))
                              .Returns(value: 0);

            int result = _sut.GetWaitingJobCount();

            Assert.AreEqual(expected: 0, result);
        }
    }
}