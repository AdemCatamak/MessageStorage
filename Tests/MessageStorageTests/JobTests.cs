using System;
using System.Threading;
using MessageStorage;
using Xunit;

namespace MessageStorageTests
{
    public class JobTests
    {
        [Fact]
        public void WhenJobObjectCreatedWithMessage__AnyFieldShouldNotBeNull()
        {
            DateTime functionStartTime = DateTime.UtcNow;
            var someEntity = new SomeEntityCreatedEvent();

            const string traceId = "trace-id";
            var message = new Message(someEntity, traceId);
            message.SetId(3);

            const string assignedHandler = "assigned-handler";
            var job = new Job(message, assignedHandler);

            Assert.NotNull(job);

            Assert.NotNull(job.TraceId);
            Assert.Equal(traceId, message.TraceId);
            Assert.Equal(traceId, job.TraceId);
            Assert.Equal(3, message.Id);
            Assert.Equal(3, job.MessageId);
            Assert.Equal(assignedHandler, job.AssignedHandlerName);
            Assert.NotNull(job.LastOperationInfo);
            Assert.True(functionStartTime <= job.LastOperationTime);
            Assert.Equal(JobStatuses.Waiting, job.JobStatus);
        }

        [Fact]
        public void WhenJobObjectCreatedWithPrimitiveFields__AllFieldShouldBeInitialized()
        {
            DateTime functionStartTime = DateTime.UtcNow;
            var someEntity = new SomeEntityCreatedEvent();

            const string traceId = "trace-id";
            var message = new Message(someEntity, traceId);
            message.SetId(3);

            const string assignedHandler = "assigned-handler";
            var job = new Job(5, message, assignedHandler, JobStatuses.InProgress, DateTime.UtcNow, "last-operation");

            Assert.NotNull(job);

            Assert.Equal(5, job.Id);
            Assert.NotNull(message.TraceId);
            Assert.Equal(traceId, job.TraceId);
            Assert.NotNull(job.AssignedHandlerName);
            Assert.Equal(assignedHandler, job.AssignedHandlerName);
            Assert.Equal(JobStatuses.InProgress, job.JobStatus);
            Assert.Equal("last-operation", job.LastOperationInfo);
            Assert.True(functionStartTime <= job.LastOperationTime);
        }

        [Fact]
        void WhenSetIdMethodIsUsed__IdVariableShouldBeChanged()
        {
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            Assert.Equal(0, job.Id);

            job.SetId(4);

            Assert.Equal(4, job.Id);
        }

        [Fact]
        void WhenSetInProgressMethodIsUsed__JobStatusShouldBeChanged()
        {
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            Assert.Equal(JobStatuses.Waiting, job.JobStatus);

            job.SetInProgress();

            Assert.Equal(JobStatuses.InProgress, job.JobStatus);
        }

        [Fact]
        void WhenSetInProgressMethodIsUsedWithLastOperationInfo__JobStatusShouldBeChanged_and_LastOperationInfoShouldBeChanged()
        {
            const string lastOperationInfo = "Job status change";
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            DateTime lastOperationTime = job.LastOperationTime;
            Assert.Equal(JobStatuses.Waiting, job.JobStatus);
            Assert.NotEqual(lastOperationInfo, job.LastOperationInfo);

            Thread.Sleep(1);
            job.SetInProgress(lastOperationInfo);

            Assert.Equal(JobStatuses.InProgress, job.JobStatus);
            Assert.Equal(lastOperationInfo, job.LastOperationInfo);
            Assert.True(lastOperationTime < job.LastOperationTime);
        }

        [Fact]
        void WhenSetDoneMethodIsUsed__JobStatusShouldBeChanged()
        {
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            Assert.Equal(JobStatuses.Waiting, job.JobStatus);

            job.SetDone();

            Assert.Equal(JobStatuses.Done, job.JobStatus);
        }

        [Fact]
        void WhenSetDoneMethodIsUsedWithLastOperationInfo__JobStatusShouldBeChanged_and_LastOperationInfoShouldBeChanged()
        {
            const string lastOperationInfo = "Job status change";
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            DateTime lastOperationTime = job.LastOperationTime;
            Assert.Equal(JobStatuses.Waiting, job.JobStatus);
            Assert.NotEqual(lastOperationInfo, job.LastOperationInfo);

            Thread.Sleep(1);
            job.SetDone(lastOperationInfo);

            Assert.Equal(JobStatuses.Done, job.JobStatus);
            Assert.Equal(lastOperationInfo, job.LastOperationInfo);
            Assert.True(lastOperationTime < job.LastOperationTime);
        }

        [Fact]
        void WhenSetFailedMethodIsUsedWithLastOperationInfo__JobStatusShouldBeChanged_and_LastOperationInfoShouldBeChanged()
        {
            const string lastOperationInfo = "Job status change";
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);
            var job = new Job(message, "assigned-handler");

            DateTime lastOperationTime = job.LastOperationTime;
            Assert.Equal(JobStatuses.Waiting, job.JobStatus);
            Assert.NotEqual(lastOperationInfo, job.LastOperationInfo);

            Thread.Sleep(1);
            job.SetFailed(lastOperationInfo);

            Assert.Equal(JobStatuses.Failed, job.JobStatus);
            Assert.Equal(lastOperationInfo, job.LastOperationInfo);
            Assert.True(lastOperationTime < job.LastOperationTime);
        }

        private interface IEvent
        {
        }

        private class SomeEntityCreatedEvent : IEvent
        {
        }
    }
}