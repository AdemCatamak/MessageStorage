using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.Db.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection;
using MessageStorage.Db.DataAccessSection.Repositories;
using MessageStorage.Models;
using Moq;
using NUnit.Framework;

namespace MessageStorage.Db.UnitTests.MessageStorageDbClientTests
{
    public class MessageStorageDbClient_Add_Test
    {
        public class DummyDbRepositoryConfiguration : DbRepositoryConfiguration
        {
        }

        private MessageStorageDbClient<DummyDbRepositoryConfiguration> _sut;
        private Mock<IHandlerManager> _mockHandlerManager;
        private Mock<IDbRepositoryContext<DummyDbRepositoryConfiguration>> _mockDbRepositoryContext;

        private Mock<IDbJobRepository<DummyDbRepositoryConfiguration>> _dbJobRepositoryMock;
        private Mock<IDbMessageRepository<DummyDbRepositoryConfiguration>> _dbMessageRepositoryMock;
        private Mock<IDbTransaction> _dbTransactionMock;

        [SetUp]
        public void SetUp()
        {
            _dbJobRepositoryMock = new Mock<IDbJobRepository<DummyDbRepositoryConfiguration>>();
            _dbMessageRepositoryMock = new Mock<IDbMessageRepository<DummyDbRepositoryConfiguration>>();
            _mockDbRepositoryContext = new Mock<IDbRepositoryContext<DummyDbRepositoryConfiguration>>();
            _mockDbRepositoryContext.Setup(context => context.DbMessageRepository)
                                    .Returns(_dbMessageRepositoryMock.Object);
            _mockDbRepositoryContext.Setup(context => context.MessageRepository)
                                    .Returns(_dbMessageRepositoryMock.Object);
            _mockDbRepositoryContext.Setup(context => context.DbJobRepository)
                                    .Returns(_dbJobRepositoryMock.Object);
            _mockDbRepositoryContext.Setup(context => context.JobRepository)
                                    .Returns(_dbJobRepositoryMock.Object);
            _dbTransactionMock = new Mock<IDbTransaction>();
            _mockDbRepositoryContext.Setup(context => context.BeginTransaction(It.IsAny<IsolationLevel>()))
                                    .Returns(_dbTransactionMock.Object);

            _mockHandlerManager = new Mock<IHandlerManager>();
            _sut = new MessageStorageDbClient<DummyDbRepositoryConfiguration>(_mockHandlerManager.Object, _mockDbRepositoryContext.Object, new MessageStorageDbConfiguration());
        }

        [Test]
        public void WhenCompatibleHandlerDoesNotExist__JobListShouldBeEmpty()
        {
            (Message message, IEnumerable<Job> jobs) = _sut.Add(null as string);

            Assert.NotNull(message);
            Assert.IsEmpty(jobs.ToList());
        }

        [Test]
        public void WhenCompatibleHandlerExist_AutoJobCreationIsFalse__GetAvailableHandlerNamesNotExecuted()
        {
            (Message message, IEnumerable<Job> jobs) = _sut.Add(null as string, autoJobCreation: false);

            List<Job> jobList = jobs?.ToList() ?? new List<Job>();

            Assert.NotNull(message);
            Assert.IsEmpty(jobList);
        }

        [Test]
        public void WhenCompatibleHandlerExist__XNumberOfJobCreated()
        {
            var handlerNames = new List<string>
                               {
                                   "handler-1",
                                   "handler-2",
                               };
            _mockHandlerManager.Setup(manager => manager.GetAvailableHandlerNames(It.IsAny<string>()))
                               .Returns(() => handlerNames);

            (Message message, IEnumerable<Job> jobs) = _sut.Add(null as string);

            List<Job> jobList = jobs?.ToList() ?? new List<Job>();

            Assert.NotNull(message);
            Assert.IsNotEmpty(jobList);
            Assert.AreEqual(handlerNames.Count, jobList.Count);

            _dbJobRepositoryMock.Verify(repository => repository.Add(It.IsAny<Job>()), Times.Exactly(handlerNames.Count));
        }
    }
}