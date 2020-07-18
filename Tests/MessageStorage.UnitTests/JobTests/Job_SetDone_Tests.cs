using System;
using System.Threading;
using MessageStorage.Models;
using NUnit.Framework;

namespace MessageStorage.UnitTests.JobTests
{
    public class Job_SetDone_Tests
    {
        private interface IEvent
        {
        }

        private class SomeEntityCreatedEvent : IEvent
        {
        }


        [Test]
        public void WhenSetDoneMethodIsUsed__JobStatusShouldBeChanged()
        {
            const string lastOperationInfo = "Job status change";
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job("assigned-handler", message);

            DateTime lastOperationTime = job.LastOperationTime;
            Assert.AreEqual(JobStatus.Waiting, job.JobStatus);
            Assert.AreNotEqual(lastOperationInfo, job.LastOperationInfo);

            Thread.Sleep(millisecondsTimeout: 1);
            job.SetDone();

            Assert.AreEqual(JobStatus.Done, job.JobStatus);
            Assert.True(lastOperationTime < job.LastOperationTime);
        }
    }
}