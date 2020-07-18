using System;
using System.Threading;
using MessageStorage.Models;
using NUnit.Framework;

namespace MessageStorage.UnitTests.JobTests
{
    public class Job_SetInProgress_Tests
    {
        private interface IEvent
        {
        }

        private class SomeEntityCreatedEvent : IEvent
        {
        }

        [Test]
        public void WhenSetInProgressMethodIsUsed__JobStatusShouldBeChanged()
        {
            const string lastOperationInfo = "Job status change";
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job("assigned-handler", message);


            DateTime lastOperationTime = job.LastOperationTime;
            Assert.AreEqual(JobStatus.Waiting, job.JobStatus);
            Assert.AreNotEqual(lastOperationInfo, job.LastOperationInfo);

            Thread.Sleep(millisecondsTimeout: 1);
            job.SetInProgress();

            Assert.AreEqual(JobStatus.InProgress, job.JobStatus);
            Assert.True(lastOperationTime < job.LastOperationTime);
        }
    }
}