using System;
using System.Collections.Generic;
using MessageStorage;
using MessageStorage.Db;
using MessageStorage.Db.DataAccessLayer;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.MessageStorageDbClientTests
{
    public class MessageStorageDbClient_Constructor_Test
    {
        private Mock<IHandlerManager> _handlerManagerMock;
        private Mock<IDbRepositoryResolver> _dbRepositoryResolverMock;
        private Mock<IMigrationRunner> _migrationRunnerMock;

        [SetUp]
        public void SetUp()
        {
            _handlerManagerMock = new Mock<IHandlerManager>();
            _dbRepositoryResolverMock = new Mock<IDbRepositoryResolver>();
            _migrationRunnerMock = new Mock<IMigrationRunner>();
        }

        [Test]
        public void WhenMigrationExecutionThrowsException__MessageStorageDbClientCouldNotCreated()
        {
            _migrationRunnerMock.Setup(runner => runner.Run(It.IsAny<IEnumerable<IMigration>>(), It.IsAny<MessageStorageDbConfiguration>()))
                                .Throws<ApplicationException>();

            MessageStorageDbClient<MessageStorageDbConfiguration> messageStorageDbClient = null;
            Assert.Throws<ApplicationException>(() => { messageStorageDbClient = new MessageStorageDbClient<MessageStorageDbConfiguration>(_handlerManagerMock.Object, _dbRepositoryResolverMock.Object, _migrationRunnerMock.Object, new List<IMigration>(), messageStorageDbConfiguration: null); });

            Assert.Null(messageStorageDbClient);
        }

        [Test]
        public void WhenMigrationExecuted__MessageStorageDbClientReadyForUsage()
        {
            var messageStorageDbClient = new MessageStorageDbClient<MessageStorageDbConfiguration>(_handlerManagerMock.Object, _dbRepositoryResolverMock.Object, _migrationRunnerMock.Object, new List<IMigration>(), messageStorageDbConfiguration: null);

            Assert.NotNull(messageStorageDbClient);
        }
    }
}