using System.Collections.Generic;
using MessageStorage;
using MessageStorage.Exceptions;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.HandlerManagerTests
{
    public class HandlerManager_GetHandlerTests
    {
        private HandlerManager _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new HandlerManager();
        }

        [Test, Sequential]
        public void WhenHandlerNameIsNullOrEmpty__HandlerNotFoundExceptionOccurs([Values("", null)] string handlerName)
        {
            Assert.Throws<HandlerNotFoundException>(() => _sut.GetHandler(handlerName));
        }

        [Test]
        public void WhenHandlersIsEmpty__HandlerNotFoundExceptionOccurs()
        {
            Assert.Throws<HandlerNotFoundException>(() => _sut.GetHandler("some-handler"));
        }

        [Test]
        public void WhenHandlersDoesNotEmpty_and_SuppliedNameNotMatch__HandlerNotFoundExceptionOccurs()
        {
            var mockHandler = new Mock<Handler>();
            mockHandler.SetupGet(h => h.Name)
                       .Returns("handler-name");

            _sut = new HandlerManager(new List<Handler>() {mockHandler.Object});

            Assert.Throws<HandlerNotFoundException>(() => _sut.GetHandler("some-handler"));
        }

        [Test]
        public void WhenHandlersDoesNotEmpty_and_SuppliedNameMatch__ResponseShouldNotBeNull()
        {
            var mockHandler1 = new Mock<Handler>();
            mockHandler1.SetupGet(h => h.Name)
                        .Returns("handler-name-1");
            var mockHandler2 = new Mock<Handler>();
            mockHandler2.SetupGet(h => h.Name)
                        .Returns("handler-name-2");

            _sut = new HandlerManager(new List<Handler>() {mockHandler1.Object, mockHandler2.Object});

            Handler handler = _sut.GetHandler("handler-name-2");

            Assert.NotNull(handler);
            Assert.AreEqual(handler, mockHandler2.Object);
        }
    }
}