using System;
using TestUtility;
using Xunit;

namespace MessageStorage.UnitTest
{
    public class Job_SetInProgress_Test
    {
        [Fact]
        public void When_SetInProgress__LastOperationTime_and_LastOperationInfo_ShouldChanged()
        {
            var job = new Job(new Message("some-message"), "some-handler");
            string expectedLastOperationInfo = JobStatus.InProgress.ToString();
            DateTime expectedLastOperationTimeGreaterThan = DateTime.UtcNow;

            Assert.NotEqual(JobStatus.InProgress, job.JobStatus);
            AssertThat.GreaterThan(job.LastOperationTime, expectedLastOperationTimeGreaterThan);

            job.SetInProgress();

            Assert.Equal(JobStatus.InProgress, job.JobStatus);
            AssertThat.GreaterThan(expectedLastOperationTimeGreaterThan, job.LastOperationTime);
            Assert.Equal(expectedLastOperationInfo, job.LastOperationInfo);
        }
    }
}