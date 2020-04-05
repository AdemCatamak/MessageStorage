using System;
using System.Threading;
using MessageStorage;
using NUnit.Framework;

namespace UnitTest.MessageStorage.JobTests
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
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            Assert.AreEqual(JobStatuses.Waiting, job.JobStatus);

            job.SetInProgress();

            Assert.AreEqual(JobStatuses.InProgress, job.JobStatus);
        }

        [Test]
        public void WhenSetInProgressMethodIsUsedWithLastOperationInfo__JobStatusShouldBeChanged_and_LastOperationInfoShouldBeChanged()
        {
            const string lastOperationInfo = "Job status change";
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            DateTime lastOperationTime = job.LastOperationTime;
            Assert.AreEqual(JobStatuses.Waiting, job.JobStatus);
            Assert.AreNotEqual(lastOperationInfo, job.LastOperationInfo);

            Thread.Sleep(1);
            job.SetInProgress(lastOperationInfo);

            Assert.AreEqual(JobStatuses.InProgress, job.JobStatus);
            Assert.AreEqual(lastOperationInfo, job.LastOperationInfo);
            Assert.True(lastOperationTime < job.LastOperationTime);
        }
    }
}