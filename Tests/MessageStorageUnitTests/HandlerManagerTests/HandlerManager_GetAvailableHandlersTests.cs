using System.Collections.Generic;
using System.Linq;
using MessageStorage;
using Moq;
using Xunit;

namespace MessageStorageUnitTests.HandlerManagerTests
{
    public class HandlerManager_GetAvailableHandlersTests
    {
        private readonly HandlerManager _sut;

        public HandlerManager_GetAvailableHandlersTests()
        {
            _sut = new HandlerManager();
        }

        [Fact]
        public void WhenHandlersIsEmpty__GetAvailableHandlersReturnEmptyList()
        {
            var payload = new object();
            IEnumerable<string> availableHandlers = _sut.GetAvailableHandlerNames(payload);

            Assert.NotNull(availableHandlers);
            Assert.Empty(availableHandlers);
        }
        
        [Fact]
        public void WhenHandlersIsNotGenericTypes__GetAvailableHandlersReturnEmptyList()
        {
            var mockHandler = new Mock<Handler>();
            mockHandler.SetupGet(h => h.Name)
                       .Returns("handler-name");
            
            _sut.AddHandler(mockHandler.Object);
            
            var payload = new object();
            IEnumerable<string> availableHandlers = _sut.GetAvailableHandlerNames(payload);

            Assert.NotNull(availableHandlers);
            Assert.Empty(availableHandlers);
        }

        [Fact]
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

            _sut.AddHandler(mockNotAccessibleHandler.Object);
            _sut.AddHandler(mockIEventHandler.Object);
            _sut.AddHandler(mockICreatedEventHandler.Object);
            _sut.AddHandler(mockEventHandler.Object);
            _sut.AddHandler(mockCreatedEventHandler.Object);
            _sut.AddHandler(mockSomeCreatedEventHandler.Object);

            var payload = new SomeCreatedEvent();
            IEnumerable<string> getAvailableHandlerNameCollection = _sut.GetAvailableHandlerNames(payload)
                                                                        .ToList();

            Assert.NotNull(getAvailableHandlerNameCollection);
            Assert.NotEmpty(getAvailableHandlerNameCollection);
            Assert.Equal(expected: 5, getAvailableHandlerNameCollection.Count());
            Assert.DoesNotContain(mockNotAccessibleHandler.Name, getAvailableHandlerNameCollection);
        }

        [Fact]
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

            _sut.AddHandler(mockNotAccessibleHandler.Object);
            _sut.AddHandler(mockIEventHandler.Object);
            _sut.AddHandler(mockICreatedEventHandler.Object);
            _sut.AddHandler(mockEventHandler.Object);
            _sut.AddHandler(mockCreatedEventHandler.Object);
            _sut.AddHandler(mockSomeCreatedEventHandler.Object);

            var payload = new object();
            IEnumerable<string> getAvailableHandlerNameCollection = _sut.GetAvailableHandlerNames(payload)
                                                                        .ToList();

            Assert.NotNull(getAvailableHandlerNameCollection);
            Assert.Empty(getAvailableHandlerNameCollection);
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