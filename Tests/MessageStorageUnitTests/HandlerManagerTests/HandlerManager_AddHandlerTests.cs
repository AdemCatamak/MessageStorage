using MessageStorage;
using MessageStorage.Exceptions;
using Moq;
using Xunit;

namespace MessageStorageUnitTests.HandlerManagerTests
{
    public class HandlerManager_AddHandlerTests
    {
        private readonly HandlerManager _sut;

        public HandlerManager_AddHandlerTests()
        {
            _sut = new HandlerManager();
        }

        [Fact]
        public void WhenHandlerManagerDoesNotContainHandlerName_AfterAddOperation__HandlersContainsNewHandler()
        {
            var mockHandler = new Mock<Handler>();

            Assert.DoesNotContain(mockHandler.Object, _sut.Handlers);

            _sut.AddHandler(mockHandler.Object);

            Assert.Contains(mockHandler.Object, _sut.Handlers);
        }

        [Fact]
        public void WhenHandlerManagerContainsHandlerName_AfterAddOperation_and_SuppressExceptionIsFalse__HandlerAlreadyExistExceptionOccurs()
        {
            var mockHandler = new Mock<Handler>();
            mockHandler.SetupGet(handler => handler.Name)
                       .Returns("handler-name");
            _sut.AddHandler(mockHandler.Object);

            Assert.Contains(mockHandler.Object, _sut.Handlers);

            var mockNewHandler = new Mock<Handler>();
            mockNewHandler.SetupGet(handler => handler.Name)
                          .Returns("handler-name");

            Assert.Throws<HandlerAlreadyExist>(() => _sut.AddHandler(mockHandler.Object));
        }

        [Fact]
        public void WhenHandlerManagerContainsHandlerName_AfterAddOperation_and_SuppressExceptionIsTrue__HandlersContainsNewHandler()
        {
            var mockHandler = new Mock<Handler>();
            mockHandler.SetupGet(handler => handler.Name)
                       .Returns("handler-name");
            _sut.AddHandler(mockHandler.Object);

            Assert.Contains(mockHandler.Object, _sut.Handlers);

            var mockNewHandler = new Mock<Handler>();
            mockNewHandler.SetupGet(handler => handler.Name)
                          .Returns("handler-name");
            _sut.AddHandler(mockHandler.Object, suppressException: true);

            Assert.Contains(mockHandler.Object, _sut.Handlers);
            Assert.DoesNotContain(mockNewHandler.Object, _sut.Handlers);
        }
    }
}