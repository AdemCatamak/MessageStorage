using System.Collections.Generic;
using System.Linq;
using MessageStorage;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db;
using MessageStorage.Db.DataAccessLayer;
using MessageStorage.Db.DataAccessLayer.Repositories;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.MessageStorageDbClientTests
{
    public class MessageStorageDbClient_Add_Test
    {
        private Mock<IHandlerManager> _handlerManagerMock;
        private Mock<IMigrationRunner> _migrationRunnerMock;
        private Mock<IDbMessageRepository> _dbMessageRepositoryMock;
        private Mock<IDbJobRepository> _dbJobRepositoryMock;
        private Mock<IDbRepositoryResolver> _dbRepositoryResolverMock;

        private MessageStorageDbClient<MessageStorageDbConfiguration> _sut;

        [SetUp]
        public void SetUp()
        {
            _handlerManagerMock = new Mock<IHandlerManager>();
            _migrationRunnerMock = new Mock<IMigrationRunner>();
            _dbMessageRepositoryMock = new Mock<IDbMessageRepository>();
            _dbJobRepositoryMock = new Mock<IDbJobRepository>();
            _dbRepositoryResolverMock = new Mock<IDbRepositoryResolver>();
            _dbRepositoryResolverMock.Setup(resolver => resolver.Resolve<IMessageRepository>())
                                     .Returns(_dbMessageRepositoryMock.Object);
            _dbRepositoryResolverMock.Setup(resolver => resolver.Resolve<IJobRepository>())
                                     .Returns(_dbJobRepositoryMock.Object);

            _sut = new MessageStorageDbClient<MessageStorageDbConfiguration>(_handlerManagerMock.Object, _dbRepositoryResolverMock.Object, _migrationRunnerMock.Object, new List<IMigration>(), messageStorageDbConfiguration: null);
        }

        [Test]
        public void WhenCompatibleHandlerDoesNotExist__JobRepositoryDoesNotResolved()
        {
            (Message message, IEnumerable<Job> jobs) = _sut.Add(null as string);

            Assert.NotNull(message);
            Assert.IsEmpty(jobs.ToList());

            _dbRepositoryResolverMock.Verify(resolver => resolver.Resolve<IMessageRepository>(), Times.Once);
            _dbRepositoryResolverMock.Verify(resolver => resolver.Resolve<IJobRepository>(), Times.Never);
        }

        [Test]
        public void WhenCompatibleHandlerExist_AutoJobCreationIsFalse__GetAvailableHandlerNamesNotExecuted()
        {
            (Message message, IEnumerable<Job> jobs) = _sut.Add(null as string, autoJobCreator: false);

            List<Job> jobList = jobs?.ToList() ?? new List<Job>();

            Assert.NotNull(message);
            Assert.IsEmpty(jobList);

            _dbRepositoryResolverMock.Verify(resolver => resolver.Resolve<IMessageRepository>(), Times.Once);
            _dbRepositoryResolverMock.Verify(resolver => resolver.Resolve<IJobRepository>(), Times.Never);

            _dbMessageRepositoryMock.Verify(repository => repository.Add(It.IsAny<Message>()), Times.Once);
            _dbJobRepositoryMock.Verify(repository => repository.Add(It.IsAny<Job>()), Times.Never);

            _handlerManagerMock.Verify(manager => manager.GetAvailableHandlerNames(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void WhenCompatibleHandlerExist__XNumberOfJobCreated()
        {
            var handlerNames = new List<string>
                               {
                                   "handler-1",
                                   "handler-2",
                               };
            _handlerManagerMock.Setup(manager => manager.GetAvailableHandlerNames(It.IsAny<string>()))
                               .Returns(() => handlerNames);

            (Message message, IEnumerable<Job> jobs) = _sut.Add(null as string);

            List<Job> jobList = jobs?.ToList() ?? new List<Job>();

            Assert.NotNull(message);
            Assert.IsNotEmpty(jobList);
            Assert.AreEqual(handlerNames.Count, jobList.Count);

            _dbRepositoryResolverMock.Verify(resolver => resolver.Resolve<IMessageRepository>(), Times.Once);
            _dbRepositoryResolverMock.Verify(resolver => resolver.Resolve<IJobRepository>(), Times.Once);

            _dbMessageRepositoryMock.Verify(repository => repository.Add(It.IsAny<Message>()), Times.Once);
            _dbJobRepositoryMock.Verify(repository => repository.Add(It.IsAny<Job>()), Times.Exactly(handlerNames.Count));
        }
    }
}