using MessageStorage.Clients.Imp;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.Models;
using Moq;
using NUnit.Framework;

namespace MessageStorage.UnitTests.MessageStorageMonitorTests
{
    public class MessageStorageMonitor_GetWaitingJobCountTests
    {
        private MessageStorageMonitor<RepositoryConfiguration> _sut;
        private Mock<IRepositoryContext<RepositoryConfiguration>> _mockRepositoryContext;

        [SetUp]
        public void SetUp()
        {
            _mockRepositoryContext = new Mock<IRepositoryContext<RepositoryConfiguration>>();

            _sut = new MessageStorageMonitor<RepositoryConfiguration>(_mockRepositoryContext.Object);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsXForWaitingJobCount__ResponseShouldBeThoseValues([Values(arg1: 0, arg2: 1, arg3: -5)] int storageAdaptorResponse)
        {
            _mockRepositoryContext.Setup(adaptor => adaptor.JobRepository.GetJobCountByStatus(JobStatuses.Waiting))
                                  .Returns(storageAdaptorResponse);

            int result =  _sut.GetWaitingJobCount();

            Assert.AreEqual(storageAdaptorResponse, result);
        }

        [Test, Sequential]
        public void WhenStorageAdaptorReturnAsZero_ResponseShouldBeThoseValues([Values(arg1: 0, arg2: 1, arg3: -5)] int storageAdaptorResponse)
        {
            _mockRepositoryContext.Setup(adaptor => adaptor.JobRepository.GetJobCountByStatus(It.IsAny<JobStatuses>()))
                                  .Returns(storageAdaptorResponse);

            _mockRepositoryContext.Setup(adaptor => adaptor.JobRepository.GetJobCountByStatus(JobStatuses.Waiting))
                                  .Returns(value: 0);

            int result =  _sut.GetWaitingJobCount();

            Assert.AreEqual(expected: 0, result);
        }
    }
}