using System;
using System.Threading;
using MessageStorage.Models;
using NUnit.Framework;

namespace MessageStorage.UnitTests.JobTests
{
    public class Job_SetFailed_Tests
    {
        private interface IEvent
        {
        }

        private class SomeEntityCreatedEvent : IEvent
        {
        }

        [Test]
        public void WhenSetFailedMethodIsUsedWithLastOperationInfo__JobStatusShouldBeChanged_and_LastOperationInfoShouldBeChanged()
        {
            const string lastOperationInfo = "Job status change";
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job("assigned-handler", message);

            DateTime lastOperationTime = job.LastOperationTime;
            Assert.AreEqual(JobStatuses.Waiting, job.JobStatus);
            Assert.AreNotEqual(lastOperationInfo, job.LastOperationInfo);

            Thread.Sleep(millisecondsTimeout: 1);
            job.SetFailed(lastOperationInfo);

            Assert.AreEqual(JobStatuses.Failed, job.JobStatus);
            Assert.AreEqual(lastOperationInfo, job.LastOperationInfo);
            Assert.True(lastOperationTime < job.LastOperationTime);
        }
    }
}