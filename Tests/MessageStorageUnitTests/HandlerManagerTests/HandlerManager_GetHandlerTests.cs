using MessageStorage;
using MessageStorage.Exceptions;
using Moq;
using Xunit;

namespace MessageStorageUnitTests.HandlerManagerTests
{
    public class HandlerManager_GetHandlerTests
    {
        private readonly HandlerManager _sut;

        public HandlerManager_GetHandlerTests()
        {
            _sut = new HandlerManager();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WhenHandlerNameIsNullOrEmpty__HandlerNotFoundExceptionOccurs(string handlerName)
        {
            Assert.Throws<HandlerNotFoundException>(() => _sut.GetHandler(handlerName));
        }

        [Fact]
        public void WhenHandlersIsEmpty__HandlerNotFoundExceptionOccurs()
        {
            Assert.Throws<HandlerNotFoundException>(() => _sut.GetHandler("some-handler"));
        }

        [Fact]
        public void WhenHandlersDoesNotEmpty_and_SuppliedNameNotMatch__HandlerNotFoundExceptionOccurs()
        {
            var mockHandler = new Mock<Handler>();
            mockHandler.SetupGet(h => h.Name)
                       .Returns("handler-name");

            _sut.AddHandler(mockHandler.Object);

            Assert.Throws<HandlerNotFoundException>(() => _sut.GetHandler("some-handler"));
        }

        [Fact]
        public void WhenHandlersDoesNotEmpty_and_SuppliedNameMatch__ResponseShouldNotBeNull()
        {
            var mockHandler1 = new Mock<Handler>();
            mockHandler1.SetupGet(h => h.Name)
                        .Returns("handler-name-1");
            var mockHandler2 = new Mock<Handler>();
            mockHandler2.SetupGet(h => h.Name)
                        .Returns("handler-name-2");

            _sut.AddHandler(mockHandler1.Object);
            _sut.AddHandler(mockHandler2.Object);

            Handler handler = _sut.GetHandler("handler-name-2");

            Assert.NotNull(handler);
            Assert.Equal(handler, mockHandler2.Object);
        }
    }
}