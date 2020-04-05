using System.Collections.Generic;
using System.Linq;
using MessageStorage;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.HandlerManagerTests
{
    public class HandlerManager_ConstructorTests
    {
        [Test]
        public void WhenConstructorExecuteWithParameterless__HandlersPropertyShouldReturnEmptyList()
        {
            var sut = new HandlerManager();

            Assert.NotNull(sut.Handlers);
            Assert.IsEmpty(sut.Handlers);
        }

        [Test]
        public void WhenConstructorExecuteWithHandlerCollection_and_CollectionIsEmpty__HandlersPropertyShouldReturnEmptyList()
        {
            var sut = new HandlerManager(new List<Handler>());

            Assert.NotNull(sut.Handlers);
            Assert.IsEmpty(sut.Handlers);
        }

        [Test]
        public void WhenConstructorExecuteWithHandlerCollection_and_CollectionContainsHandlerWithTheSameName__HandlersContainsFirstOne()
        {
            var mockHandler1 = new Mock<Handler>();
            mockHandler1.SetupGet(handler => handler.Name)
                        .Returns("HandlerName");

            var mockHandler2 = new Mock<Handler>();
            mockHandler2.SetupGet(handler => handler.Name)
                        .Returns("HandlerName");

            var sut = new HandlerManager(new List<Handler>() {mockHandler1.Object, mockHandler2.Object});

            Assert.NotNull(sut.Handlers);
            Assert.IsNotEmpty(sut.Handlers);
            Assert.AreEqual(1, sut.Handlers.Count);
            Assert.AreEqual(mockHandler1.Object, sut.Handlers.First());
            Assert.AreNotEqual(mockHandler2.Object, sut.Handlers.First());
        }
    }
}