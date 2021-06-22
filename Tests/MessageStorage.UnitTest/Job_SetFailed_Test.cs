using System;
using Xunit;

namespace MessageStorage.UnitTest
{
    public class Job_SetFailed_Test
    {
        [Fact]
        public void When_SetFailed__LastOperationTime_and_LastOperationInfo_ShouldChanged()
        {
            var job = new Job(new Message("some-message"), "some-handler");
            const string expectedLastOperationInfo = "Failed Reason";
            DateTime expectedLastOperationTimeGreaterThan = DateTime.UtcNow;

            Assert.NotEqual(JobStatus.Failed, job.JobStatus);
            Assert.True(expectedLastOperationTimeGreaterThan > job.LastOperationTime);

            job.SetFailed(expectedLastOperationInfo);

            Assert.Equal(JobStatus.Failed, job.JobStatus);
            Assert.True(expectedLastOperationTimeGreaterThan < job.LastOperationTime);
            Assert.Equal(expectedLastOperationInfo, job.LastOperationInfo);
        }
    }
}