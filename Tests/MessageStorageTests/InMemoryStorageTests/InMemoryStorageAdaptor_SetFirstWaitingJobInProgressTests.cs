using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageStorage;
using Xunit;

namespace MessageStorageTests.InMemoryStorageTests
{
    // ReSharper disable once InconsistentNaming
    public class InMemoryStorageAdaptor_SetFirstWaitingJobInProgressTests
    {
        private readonly InMemoryStorageAdaptor _sut;

        public InMemoryStorageAdaptor_SetFirstWaitingJobInProgressTests()
        {
            _sut = new InMemoryStorageAdaptor();
        }

        [Fact]
        public void WhenJobStorageDoesNotContainsAnyWaitingJob__ResponseShouldBeNull()
        {
            Job job = _sut.SetFirstWaitingJobToInProgress();

            Assert.Null(job);
        }

        [Fact]
        public void WhenJobStorageDoesNotContainsWaitingJob__ResponseShouldNotBeNull()
        {
            var message = new Message(null);
            var job = new Job(message, "handler");
            _sut.Add(message, new[] {job});

            Job firstWaitingJobToInProgress = _sut.SetFirstWaitingJobToInProgress();

            Assert.NotNull(job);
            Assert.Equal(job.Id, firstWaitingJobToInProgress.Id);
            Assert.Equal(job.MessageId, firstWaitingJobToInProgress.MessageId);
            Assert.Equal(JobStatuses.InProgress, firstWaitingJobToInProgress.JobStatus);
        }

        [Fact]
        public void WhenJobStorageSetWaitingJobToInProgress__StorageShouldBeLocked()
        {
            for (int i = 0; i < 20; i++)
            {
                var message = new Message(null);
                var job = new Job(message, "handler");
                _sut.Add(message, new[] {job});
            }


            var jobConcurrentBag = new ConcurrentBag<Job>();

            Parallel.For(0, 1000, (i) =>
                                  {
                                      Job job = _sut.SetFirstWaitingJobToInProgress();
                                      jobConcurrentBag.Add(job);
                                  });

            List<Job> jobList = jobConcurrentBag.ToList();

            Assert.Equal(20, jobList.Count(j => j != null));
            Assert.Equal(1000, jobList.Count);
        }
    }
}