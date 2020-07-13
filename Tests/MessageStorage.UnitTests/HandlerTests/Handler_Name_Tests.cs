using System.Threading.Tasks;
using NUnit.Framework;

namespace MessageStorage.UnitTests.HandlerTests
{
    public class Handler_Name_Tests
    {
        private class DummyMessage
        {
        }

        private class DummyMessageHandler : Handler<DummyMessage>
        {
            protected override Task Handle(DummyMessage payload)
            {
                return Task.CompletedTask;
            }
        }

        private class DummyHandler : Handler
        {
            public override Task BaseHandleOperation(object payload) => Task.CompletedTask;
        }


        [Test]
        public void WhenNameDoesNotOverride__NameFieldShouldReturnNameSpaceAndClassName()
        {
            var dummyHandler = new DummyHandler();

            Assert.IsTrue(dummyHandler.Name.Contains(typeof(DummyHandler).Namespace));
            Assert.IsTrue(dummyHandler.Name.Contains(typeof(DummyHandler).Name));
            Assert.AreEqual(typeof(DummyHandler).FullName, dummyHandler.Name);
        }

        [Test]
        public void WhenNameDoesNotOverrideTypedHandler__NameFieldShouldReturnNameSpaceAndClassName()
        {
            var dummyMessageHandler = new DummyMessageHandler();

            Assert.IsTrue(dummyMessageHandler.Name.Contains(typeof(DummyMessageHandler).Namespace));
            Assert.IsTrue(dummyMessageHandler.Name.Contains(typeof(DummyMessageHandler).Name));

            Assert.AreEqual(typeof(DummyMessageHandler).FullName, dummyMessageHandler.Name);
        }
    }
}