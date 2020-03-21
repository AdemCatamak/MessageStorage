using MessageStorage;
using NUnit.Framework;

namespace UnitTest.MessageStorage.JobTests
{
    public class Job_SetId_Tests
    {
        [Test]
        public void WhenSetIdMethodIsUsed__IdVariableShouldBeChanged()
        {
            var message = new Message(payload: null);
            var job = new Job(message, "assigned-handler");

            Assert.AreEqual(0, job.Id);

            job.SetId(4);

            Assert.AreEqual(4, job.Id);
        }
    }
}