using System;
using System.Collections.Generic;
using System.Threading;
using TestUtility;
using Xunit;

namespace MessageStorage.UnitTest
{
    public class Job_Creation_Test
    {
        private readonly Message _message = new Message(Guid.Empty, "1", DateTime.Parse("1923-10-29"));

        [Fact]
        public void When_JobCreated__JobIdShouldNotBeNull()
        {
            Job job = new Job(_message, "some-handler");

            Assert.NotEmpty(job.Id.ToString());
        }

        [Fact]
        public void When_JobCreatedOneAfterAnother__JobIdShouldBeGreaterThanPreviousOne()
        {
            List<Job> jobs = new List<Job>();
            for (var i = 0; i < 100; i++)
            {
                jobs.Add(new Job(_message, i.ToString()));
                Thread.Sleep(1);
            }

            for (var i = 1; i < jobs.Count; i++)
            {
                Job job = jobs[i];
                Job jobPrevious = jobs[i - 1];

                AssertThat.GreaterThan(Convert.ToInt32(jobPrevious.MessageHandlerTypeName), Convert.ToInt32(job.MessageHandlerTypeName));
                AssertThat.GreaterThan(jobPrevious.CreatedOn, job.CreatedOn);
                AssertThat.GreaterThan(jobPrevious.Id, job.Id);
            }
        }

        [Fact]
        public void When_JobCreatedWithFullProperty__JobContainsAllProperty()
        {
            var jobId = Guid.NewGuid();
            const string messageHandlerTypeName = "message-handler";
            const JobStatus jobStatus = JobStatus.InProgress;
            DateTime createdOn = DateTime.Parse("1923-10-29");
            var lastOperationTime = DateTime.MaxValue;
            const string? lastOperationInfo = "last-operation-info";

            Job job = new Job(jobId, _message, messageHandlerTypeName, jobStatus, createdOn, lastOperationTime, lastOperationInfo, 5, DateTime.UtcNow);

            Assert.Equal(jobId, job.Id);
            Assert.Equal(messageHandlerTypeName, job.MessageHandlerTypeName);
            Assert.Equal(jobStatus, job.JobStatus);
            Assert.Equal(createdOn, job.CreatedOn);
            Assert.Equal(lastOperationTime, job.LastOperationTime);
            Assert.Equal(lastOperationInfo, job.LastOperationInfo);
        }
    }
}