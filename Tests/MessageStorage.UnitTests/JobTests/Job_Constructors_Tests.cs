using System;
using MessageStorage.Models;
using NUnit.Framework;

namespace MessageStorage.UnitTests.JobTests
{
    public class Job_Constructors_Tests
    {
        private interface IEvent
        {
        }

        private class SomeEntityCreatedEvent : IEvent
        {
        }

        [Test]
        public void WhenJobObjectCreatedWithMessage__AnyFieldShouldNotBeNull()
        {
            DateTime functionStartTime = DateTime.UtcNow;
            var someEntity = new SomeEntityCreatedEvent();

            var message = new Message(someEntity);

            const string assignedHandler = "assigned-handler";
            var job = new Job(assignedHandler, message);

            Assert.NotNull(job);
            Assert.IsNotEmpty(job.Id);
            Assert.IsNotEmpty(message.Id);
            Assert.AreEqual(assignedHandler, job.AssignedHandlerName);
            Assert.True(functionStartTime <= job.LastOperationTime);
            Assert.AreEqual(JobStatuses.Waiting, job.JobStatus);
        }

        [Test]
        public void WhenJobObjectCreatedWithPrimitiveFields__AllFieldShouldBeInitialized()
        {
            DateTime functionStartTime = DateTime.UtcNow;
            var someEntity = new SomeEntityCreatedEvent();

            var message = new Message(someEntity);

            const string jobId = "4";
            const string assignedHandler = "assigned-handler";
            var job = new Job(jobId, DateTime.UtcNow, assignedHandler, JobStatuses.InProgress, DateTime.UtcNow, "last-operation", message);

            Assert.NotNull(job);

            Assert.AreEqual(jobId, job.Id);
            Assert.NotNull(job.AssignedHandlerName);
            Assert.AreEqual(assignedHandler, job.AssignedHandlerName);
            Assert.AreEqual(JobStatuses.InProgress, job.JobStatus);
            Assert.AreEqual("last-operation", job.LastOperationInfo);
            Assert.True(functionStartTime <= job.LastOperationTime);
        }
    }
}