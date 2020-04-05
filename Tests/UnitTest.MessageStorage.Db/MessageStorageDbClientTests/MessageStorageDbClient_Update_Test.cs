using System.Collections.Generic;
using MessageStorage;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db;
using MessageStorage.Db.DataAccessLayer;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.MessageStorageDbClientTests
{
    public class MessageStorageDbClient_Update_Test
    {
        private Mock<IDbRepositoryResolver> _dbRepositoryResolverMock;
        private Mock<IJobRepository> _jobRepositoryMock;
        private MessageStorageDbClient<MessageStorageDbConfiguration> _sut;

        [SetUp]
        public void SetUp()
        {
            var handlerManagerMock = new Mock<IHandlerManager>();
            var migrationRunnerMock = new Mock<IMigrationRunner>();
            _jobRepositoryMock = new Mock<IJobRepository>();
            _dbRepositoryResolverMock = new Mock<IDbRepositoryResolver>();
            _dbRepositoryResolverMock.Setup(resolver => resolver.Resolve<IJobRepository>())
                                     .Returns(_jobRepositoryMock.Object);

            _sut = new MessageStorageDbClient<MessageStorageDbConfiguration>(handlerManagerMock.Object, _dbRepositoryResolverMock.Object, migrationRunnerMock.Object, new List<IMigration>(), messageStorageDbConfiguration: null);
        }

        [Test]
        public void WhenRepositoryResolverReturnRepository__UpdateJobWillBeExecuted()
        {
            _dbRepositoryResolverMock.Setup(resolver => resolver.Resolve<IJobRepository>())
                                     .Returns(_jobRepositoryMock.Object);

            _sut.Update(It.IsAny<Job>());

            _dbRepositoryResolverMock.Verify(resolver => resolver.Resolve<IJobRepository>(), Times.Once);
            _jobRepositoryMock.Verify(repository => repository.Update(It.IsAny<Job>()), Times.Once);
        }
    }
}