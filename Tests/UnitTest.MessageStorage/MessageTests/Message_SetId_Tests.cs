using MessageStorage;
using NUnit.Framework;

namespace UnitTest.MessageStorage.MessageTests
{
    public class Message_SetId_Tests
    {
        [Test]
        public void WhenSetIdMethodIsUsed__IdVariableShouldBeChanged()
        {
            var message = new Message(payload: null);

            Assert.AreEqual(0, message.Id);

            message.SetId(4);

            Assert.AreEqual(4, message.Id);
        }
    }
}