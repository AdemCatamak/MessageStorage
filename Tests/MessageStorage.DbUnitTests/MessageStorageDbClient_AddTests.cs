using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.Db;
using Moq;
using Xunit;

namespace MessageStorage.DbUnitTests
{
    public class MessageStorageDbClient_AddTests
    {
        private readonly Mock<IDbStorageAdaptor> _mockIDbStorageAdaptor;
        private readonly Mock<IHandlerManager> _mockIHandlerManager;
        private MessageStorageDbClient _sut;

        public MessageStorageDbClient_AddTests()
        {
            _mockIDbStorageAdaptor = new Mock<IDbStorageAdaptor>();
            _mockIHandlerManager = new Mock<IHandlerManager>();

            _sut = new MessageStorageDbClient(_mockIDbStorageAdaptor.Object, _mockIHandlerManager.Object);
        }

        [Fact]
        public void WhenMessageStorageDbClientAddOperationExecuted_with_IDbTransaction__DbStorageAdaptorAddOperationExecutedOnce()
        {
            var mockDbTransaction = new Mock<IDbTransaction>();

            _mockIHandlerManager.Setup(manager => manager.GetAvailableHandlerNames(It.IsAny<string>()))
                                .Returns(() => new List<string>());

            _sut.Add(null as string, mockDbTransaction.Object);

            _mockIHandlerManager.Verify(manager => manager.GetAvailableHandlerNames(It.IsAny<string>()), Times.Once());
            _mockIDbStorageAdaptor.Verify(adaptor => adaptor.Add(
                                                                 It.Is<Message>(message => message.Payload == null),
                                                                 It.Is<IEnumerable<Job>>(jobs => jobs.Any() == false),
                                                                 mockDbTransaction.Object));
        }

        [Fact]
        public void WhenMessageStorageDbClientAddOperationExecuted_with_IDbTransaction_and_Jobs__DbStorageAdaptorAddOperationExecutedOnce()
        {
            var mockDbTransaction = new Mock<IDbTransaction>();

            const string handlerName = "some-handler";
            _mockIHandlerManager.Setup(manager => manager.GetAvailableHandlerNames(It.IsAny<int>()))
                                .Returns(() => new List<string>()
                                               {
                                                   handlerName
                                               });

            _sut.Add(5, mockDbTransaction.Object);

            _mockIHandlerManager.Verify(manager => manager.GetAvailableHandlerNames(It.IsAny<int>()), Times.Once());
            _mockIDbStorageAdaptor.Verify(adaptor => adaptor.Add(
                                                                 It.Is<Message>(message => message.Payload != null),
                                                                 It.Is<IEnumerable<Job>>(jobs => jobs.Count() == 1
                                                                                              && jobs.First().AssignedHandlerName == handlerName),
                                                                 mockDbTransaction.Object));
        }
    }
}