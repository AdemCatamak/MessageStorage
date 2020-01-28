using System.Threading.Tasks;
using MessageStorage;
using MessageStorage.Exceptions;
using Xunit;

namespace MessageStorageUnitTests
{
    public class HandlerTests
    {
        public static bool DummyMessageHandlerCouldHandle = false;

        [Fact]
        public void WhenNameDoesNotOverride__NameFieldShouldReturnNameSpaceAndClassName()
        {
            var dummyHandler = new DummyHandler();

            Assert.Contains(typeof(DummyHandler).Namespace, dummyHandler.Name);
            Assert.Contains(typeof(DummyHandler).Name, dummyHandler.Name);
            Assert.Equal(typeof(DummyHandler).FullName, dummyHandler.Name);
        }

        [Fact]
        public void WhenNameDoesNotOverrideTypedHandler__NameFieldShouldReturnNameSpaceAndClassName()
        {
            var dummyMessageHandler = new DummyMessageHandler();

            Assert.Contains(typeof(DummyMessageHandler).Namespace, dummyMessageHandler.Name);
            Assert.Contains(typeof(DummyMessageHandler).Name, dummyMessageHandler.Name);
            Assert.Equal(typeof(DummyMessageHandler).FullName, dummyMessageHandler.Name);
        }

        [Fact]
        public async Task WhenTypedHandlerGetNotCompatiblePayload__ArgumentNotCompatibleExceptionOccurs()
        {
            var dummyMessageHandler = new DummyMessageHandler();
            await Assert.ThrowsAsync<ArgumentNotCompatibleException>(async () => await dummyMessageHandler.Handle(5));
        }

        [Fact]
        public async Task WhenTypedHandlerGetValidPayload__TypedPayloadWillBeExecuted()
        {
            var dummyMessageHandler = new DummyMessageHandler();
            await dummyMessageHandler.Handle(new DummyMessage());
            Assert.True(DummyMessageHandlerCouldHandle);
        }

        private class DummyHandler : Handler
        {
            public override Task Handle(object payload) => Task.CompletedTask;
        }

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : Handler<DummyMessage>
        {
            protected override Task Handle(DummyMessage payload)
            {
                DummyMessageHandlerCouldHandle = true;
                return Task.CompletedTask;
            }
        }
    }
}