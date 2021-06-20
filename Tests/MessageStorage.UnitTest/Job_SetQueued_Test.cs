using System;
using TestUtility;
using Xunit;

namespace MessageStorage.UnitTest
{
    public class Job_SetQueued_Test
    {
        [Fact]
        public void When_SetQueued__LastOperationTime_and_LastOperationInfo_ShouldChanged()
        {
            var job = new Job(Guid.NewGuid(),
                              new Message("some-message"),
                              "some-handler",
                              JobStatus.InProgress,
                              DateTime.UtcNow,
                              DateTime.UtcNow,
                              "last-operation",
                              5,
                              DateTime.UtcNow);

            AsyncHelper.WaitFor(TimeSpan.FromMilliseconds(10));

            string expectedLastOperationInfo = JobStatus.Queued.ToString();
            DateTime expectedLastOperationTimeGreaterThan = DateTime.UtcNow;

            Assert.NotEqual(JobStatus.Queued, job.JobStatus);
            AssertThat.LessThan(expectedLastOperationTimeGreaterThan, job.LastOperationTime);

            AsyncHelper.WaitFor(TimeSpan.FromMilliseconds(10));
            job.SetQueued();


            Assert.Equal(JobStatus.Queued, job.JobStatus);
            AssertThat.GreaterThan(expectedLastOperationTimeGreaterThan, job.LastOperationTime);
            Assert.Equal(expectedLastOperationInfo, job.LastOperationInfo);
        }
    }
}