using System.Threading.Tasks;
using MessageStorage.Exceptions;
using NUnit.Framework;

namespace MessageStorage.UnitTests.HandlerTests
{
    public class Handler_BaseHandle_Tests
    {
        private class DummyMessage
        {
        }

        private class DummyMessageHandler : Handler<DummyMessage>
        {
            protected override Task Handle(DummyMessage payload)
            {
                _dummyMessageHandlerCouldHandle = true;
                return Task.CompletedTask;
            }
        }

        private static bool _dummyMessageHandlerCouldHandle = false;

        [Test]
        public void WhenTypedHandlerGetNotCompatiblePayload__ArgumentNotCompatibleExceptionOccurs()
        {
            var dummyMessageHandler = new DummyMessageHandler();
            Assert.ThrowsAsync<ArgumentNotCompatibleException>(async () => await dummyMessageHandler.BaseHandleOperation(payload: 5));
        }

        [Test]
        public async Task WhenTypedHandlerGetValidPayload__TypedPayloadWillBeExecuted()
        {
            var dummyMessageHandler = new DummyMessageHandler();
            await dummyMessageHandler.BaseHandleOperation(new DummyMessage());
            Assert.True(_dummyMessageHandlerCouldHandle);
        }
    }
}