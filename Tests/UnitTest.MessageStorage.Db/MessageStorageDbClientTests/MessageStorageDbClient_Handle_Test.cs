using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageStorage;
using MessageStorage.Db;
using MessageStorage.Db.DataAccessLayer;
using MessageStorage.Exceptions;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.MessageStorageDbClientTests
{
    public class MessageStorageDbClient_Handle_Test
    {
        private class DummyHandler : Handler<string>
        {
            private readonly Action _handleAction;

            public DummyHandler(Action handleAction)
            {
                _handleAction = handleAction;
            }

            protected override Task Handle(string payload)
            {
                _handleAction();
                return Task.CompletedTask;
            }
        }

        private Mock<IHandlerManager> _handlerManagerMock;

        private MessageStorageDbClient<MessageStorageDbConfiguration> _sut;

        [SetUp]
        public void SetUp()
        {
            _handlerManagerMock = new Mock<IHandlerManager>();
            var migrationRunnerMock = new Mock<IMigrationRunner>();
            var dbRepositoryResolverMock = new Mock<IDbRepositoryResolver>();

            _sut = new MessageStorageDbClient<MessageStorageDbConfiguration>(_handlerManagerMock.Object, dbRepositoryResolverMock.Object, migrationRunnerMock.Object, new List<IMigration>(), messageStorageDbConfiguration: null);
        }

        [Test]
        public void GetHandlerReturnNullAsResponse__HandlerNotFoundExceptionOccurs()
        {
            _handlerManagerMock.Setup(manager => manager.GetHandler(It.IsAny<string>()))
                               .Returns(null as Handler);

            var message = new Message("some-message");
            var job = new Job(message, "handler-1");

            Assert.ThrowsAsync<HandlerNotFoundException>(async () => await _sut.Handle(job));
        }

        [Test]
        public async Task GetHandlerReturnHandler__HandlerBaseHandleOperationExecuted()
        {
            var executed = false;

            void HandleAction()
            {
                executed = true;
            }

            _handlerManagerMock.Setup(manager => manager.GetHandler(It.IsAny<string>()))
                               .Returns(new DummyHandler(HandleAction));

            var message = new Message("some-message");
            var job = new Job(message, "handler-1");

            await _sut.Handle(job);

            Assert.IsTrue(executed);
        }
    }
}