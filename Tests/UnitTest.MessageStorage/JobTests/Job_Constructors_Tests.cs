using System;
using MessageStorage;
using NUnit.Framework;
using Message = MessageStorage.Message;

namespace UnitTest.MessageStorage.JobTests
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

            const string traceId = "trace-id";
            var message = new Message(someEntity, traceId);

            const string assignedHandler = "assigned-handler";
            var job = new Job(message, assignedHandler);

            Assert.NotNull(job);
            Assert.NotNull(job.JobId);
            Assert.IsNotEmpty(job.JobId);
            Assert.NotNull(job.TraceId);
            Assert.AreEqual(traceId, message.TraceId);
            Assert.AreEqual(traceId, job.TraceId);
            Assert.NotNull(message.MessageId);
            Assert.IsNotEmpty(message.MessageId);
            Assert.AreEqual(assignedHandler, job.AssignedHandlerName);
            Assert.NotNull(job.LastOperationInfo);
            Assert.True(functionStartTime <= job.LastOperationTime);
            Assert.AreEqual(JobStatuses.Waiting, job.JobStatus);
        }

        [Test]
        public void WhenJobObjectCreatedWithPrimitiveFields__AllFieldShouldBeInitialized()
        {
            DateTime functionStartTime = DateTime.UtcNow;
            var someEntity = new SomeEntityCreatedEvent();

            const string traceId = "trace-id";
            var message = new Message(someEntity, traceId);

            const string jobId = "4";
            const string assignedHandler = "assigned-handler";
            var job = new Job(jobId, message, assignedHandler, JobStatuses.InProgress, DateTime.UtcNow, "last-operation");

            Assert.NotNull(job);

            Assert.AreEqual(jobId, job.JobId);
            Assert.NotNull(message.TraceId);
            Assert.AreEqual(traceId, job.TraceId);
            Assert.NotNull(job.AssignedHandlerName);
            Assert.AreEqual(assignedHandler, job.AssignedHandlerName);
            Assert.AreEqual(JobStatuses.InProgress, job.JobStatus);
            Assert.AreEqual("last-operation", job.LastOperationInfo);
            Assert.True(functionStartTime <= job.LastOperationTime);
        }
    }
}