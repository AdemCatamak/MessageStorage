using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace MessageStorage.UnitTests.HandlerManagerTests
{
    public class HandlerManager_GetAvailableHandlersTests
    {
        private HandlerManager _sut;


        [Test]
        public void WhenHandlersIsEmpty__GetAvailableHandlersReturnEmptyList()
        {
            _sut = new HandlerManager();
            var payload = new object();
            IEnumerable<string> availableHandlers = _sut.GetAvailableHandlerNames(payload);

            Assert.NotNull(availableHandlers);
            Assert.IsEmpty(availableHandlers);
        }

        [Test]
        public void WhenHandlersIsNotGenericTypes__GetAvailableHandlersReturnEmptyList()
        {
            var mockHandler = new Mock<Handler>();
            mockHandler.SetupGet(h => h.Name)
                       .Returns("handler-name");

            _sut = new HandlerManager(new List<Handler> {mockHandler.Object});

            var payload = new object();
            IEnumerable<string> availableHandlers = _sut.GetAvailableHandlerNames(payload);

            Assert.NotNull(availableHandlers);
            Assert.IsEmpty(availableHandlers);
        }

        [Test]
        public void WhenHandlerGenericTypeIsAssignableFromPayloadType__GetAvailableHandlersReturnAllOfThem()
        {
            var mockNotAccessibleHandler = new Mock<Handler<HandlerManager_GetAvailableHandlersTests>>();
            mockNotAccessibleHandler.SetupGet(handler => handler.Name)
                                    .Returns(nameof(mockNotAccessibleHandler));
            var mockIEventHandler = new Mock<Handler<IEvent>>();
            mockIEventHandler.SetupGet(handler => handler.Name)
                             .Returns(nameof(mockIEventHandler));
            var mockICreatedEventHandler = new Mock<Handler<ICreatedEvent>>();
            mockICreatedEventHandler.SetupGet(handler => handler.Name)
                                    .Returns(nameof(mockICreatedEventHandler));
            var mockEventHandler = new Mock<Handler<Event>>();
            mockEventHandler.SetupGet(handler => handler.Name)
                            .Returns(nameof(mockEventHandler));
            var mockCreatedEventHandler = new Mock<Handler<CreatedEvent>>();
            mockCreatedEventHandler.SetupGet(handler => handler.Name)
                                   .Returns(nameof(mockCreatedEventHandler));
            var mockSomeCreatedEventHandler = new Mock<Handler<SomeCreatedEvent>>();
            mockSomeCreatedEventHandler.SetupGet(handler => handler.Name)
                                       .Returns(nameof(mockSomeCreatedEventHandler));

            _sut = new HandlerManager(new List<Handler>
                                      {
                                          mockNotAccessibleHandler.Object,
                                          mockIEventHandler.Object,
                                          mockICreatedEventHandler.Object,
                                          mockEventHandler.Object,
                                          mockCreatedEventHandler.Object,
                                          mockSomeCreatedEventHandler.Object
                                      });


            var payload = new SomeCreatedEvent();
            IEnumerable<string> getAvailableHandlerNameCollection = _sut.GetAvailableHandlerNames(payload)
                                                                        .ToList();

            Assert.NotNull(getAvailableHandlerNameCollection);
            Assert.IsNotEmpty(getAvailableHandlerNameCollection);
            Assert.AreEqual(expected: 5, getAvailableHandlerNameCollection.Count());
            Assert.IsTrue(getAvailableHandlerNameCollection.All(s => s != mockNotAccessibleHandler.Name));
        }

        [Test]
        public void WhenHandlerGenericTypeIsNotAssignableFromPayloadType__GetAvailableHandlersReturnEmpty()
        {
            var mockNotAccessibleHandler = new Mock<Handler<HandlerManager_GetAvailableHandlersTests>>();
            mockNotAccessibleHandler.SetupGet(handler => handler.Name)
                                    .Returns(nameof(mockNotAccessibleHandler));
            var mockIEventHandler = new Mock<Handler<IEvent>>();
            mockIEventHandler.SetupGet(handler => handler.Name)
                             .Returns(nameof(mockIEventHandler));
            var mockICreatedEventHandler = new Mock<Handler<ICreatedEvent>>();
            mockICreatedEventHandler.SetupGet(handler => handler.Name)
                                    .Returns(nameof(mockICreatedEventHandler));
            var mockEventHandler = new Mock<Handler<Event>>();
            mockEventHandler.SetupGet(handler => handler.Name)
                            .Returns(nameof(mockEventHandler));
            var mockCreatedEventHandler = new Mock<Handler<CreatedEvent>>();
            mockCreatedEventHandler.SetupGet(handler => handler.Name)
                                   .Returns(nameof(mockCreatedEventHandler));
            var mockSomeCreatedEventHandler = new Mock<Handler<SomeCreatedEvent>>();
            mockSomeCreatedEventHandler.SetupGet(handler => handler.Name)
                                       .Returns(nameof(mockSomeCreatedEventHandler));

            var handlers = new List<Handler>
                           {
                               mockNotAccessibleHandler.Object,
                               mockIEventHandler.Object,
                               mockICreatedEventHandler.Object,
                               mockEventHandler.Object,
                               mockCreatedEventHandler.Object,
                               mockSomeCreatedEventHandler.Object
                           };
            _sut = new HandlerManager(handlers);
            var payload = new object();
            IEnumerable<string> getAvailableHandlerNameCollection = _sut.GetAvailableHandlerNames(payload)
                                                                        .ToList();

            Assert.NotNull(getAvailableHandlerNameCollection);
            Assert.IsEmpty(getAvailableHandlerNameCollection);
        }

        public interface IEvent
        {
        }

        public interface ICreatedEvent : IEvent
        {
        }

        public abstract class Event : IEvent
        {
        }

        public abstract class CreatedEvent : Event, ICreatedEvent
        {
        }

        public class SomeCreatedEvent : CreatedEvent
        {
        }
    }
}