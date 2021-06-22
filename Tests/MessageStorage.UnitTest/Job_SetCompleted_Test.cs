using System;
using TestUtility;
using Xunit;

namespace MessageStorage.UnitTest
{
    public class Job_SetCompleted_Test
    {
        [Fact]
        public void When_SetCompleted__LastOperationTime_and_LastOperationInfo_ShouldChanged()
        {
            var job = new Job(new Message("some-message"), "some-handler");
            string expectedLastOperationInfo = JobStatus.Completed.ToString();
            DateTime expectedLastOperationTimeGreaterThan = DateTime.UtcNow;

            Assert.NotEqual(JobStatus.Completed, job.JobStatus);
            AssertThat.GreaterThan(job.LastOperationTime, expectedLastOperationTimeGreaterThan);

            job.SetCompleted();

            Assert.Equal(JobStatus.Completed, job.JobStatus);
            AssertThat.GreaterThan(expectedLastOperationTimeGreaterThan, job.LastOperationTime);
            Assert.Equal(expectedLastOperationInfo, job.LastOperationInfo);
        }
    }
}