using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using MessageStorage.Models;
using Moq;
using NUnit.Framework;

namespace MessageStorage.UnitTests.MessageStorageMonitorTests
{
    public class MessageStorageMonitor_GetFailedJobCountTests
    {
        private MessageStorageMonitor _sut;
        private Mock<IRepositoryContext> _mockRepositoryContext;

        [SetUp]
        public void SetUp()
        {
            _mockRepositoryContext = new Mock<IRepositoryContext>();

            _sut = new MessageStorageMonitor(_mockRepositoryContext.Object);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsXForFailedJobCount__ResponseShouldBeThoseValues([Values(arg1: 0, arg2: 1, arg3: -5)] int storageAdaptorResponse)
        {
            _mockRepositoryContext.Setup(context => context.JobRepository.GetJobCountByStatus(JobStatuses.Failed))
                                  .Returns(storageAdaptorResponse);

            int result = _sut.GetFailedJobCount();

            Assert.AreEqual(storageAdaptorResponse, result);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsZeroForFailedJob_ResponseShouldBeThoseValues([Values(arg1: 0, arg2: 1, arg3: -5)] int storageAdaptorResponse)
        {
            _mockRepositoryContext.Setup(context => context.JobRepository.GetJobCountByStatus(It.IsAny<JobStatuses>()))
                                  .Returns(storageAdaptorResponse);

            _mockRepositoryContext.Setup(adaptor => adaptor.JobRepository.GetJobCountByStatus(JobStatuses.Failed))
                                  .Returns(value: 0);

            int result = _sut.GetFailedJobCount();

            Assert.AreEqual(expected: 0, result);
        }
    }
}