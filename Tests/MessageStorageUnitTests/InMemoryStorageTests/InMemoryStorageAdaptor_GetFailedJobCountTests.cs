using MessageStorage;
using Xunit;

namespace MessageStorageUnitTests.InMemoryStorageTests
{
    public class InMemoryStorageAdaptor_GetFailedJobCountTests
    {
        private readonly InMemoryStorageAdaptor _sut;

        public InMemoryStorageAdaptor_GetFailedJobCountTests()
        {
            _sut = new InMemoryStorageAdaptor();
        }

        [Fact]
        public void WhenThereIsNoJobInStorage__JobCountShouldBe0ForAllStatuses()
        {
            int waitingJobCount = _sut.GetJobCountByStatus(JobStatuses.Waiting);
            int inProgressJobCount = _sut.GetJobCountByStatus(JobStatuses.InProgress);
            int failedJobCount = _sut.GetJobCountByStatus(JobStatuses.Failed);
            int doneJobCount = _sut.GetJobCountByStatus(JobStatuses.Done);

            Assert.Equal(expected: 0, waitingJobCount);
            Assert.Equal(expected: 0, inProgressJobCount);
            Assert.Equal(expected: 0, failedJobCount);
            Assert.Equal(expected: 0, doneJobCount);
        }

        [Fact]
        public void WhenThereIsOneJobInStorage_and_ItIsWaiting__OnlyFailedJobCountShouldBe1()
        {
            var message = new Message(null);
            var job = new Job(message, "handler-name");
            _sut.Add(message, new[] {job});

            int waitingJobCount = _sut.GetJobCountByStatus(JobStatuses.Waiting);
            int inProgressJobCount = _sut.GetJobCountByStatus(JobStatuses.InProgress);
            int failedJobCount = _sut.GetJobCountByStatus(JobStatuses.Failed);
            int doneJobCount = _sut.GetJobCountByStatus(JobStatuses.Done);

            Assert.Equal(expected: 1, waitingJobCount);
            Assert.Equal(expected: 0, inProgressJobCount);
            Assert.Equal(expected: 0, failedJobCount);
            Assert.Equal(expected: 0, doneJobCount);
        }

        [Fact]
        public void WhenThereIsOneJobInStorage_and_ItIsFailed__OnlyFailedJobCountShouldBe1()
        {
            var message = new Message(null);
            var job = new Job(message, "handler-name");
            job.SetFailed("some-reason");
            _sut.Add(message, new[] {job});

            int waitingJobCount = _sut.GetJobCountByStatus(JobStatuses.Waiting);
            int inProgressJobCount = _sut.GetJobCountByStatus(JobStatuses.InProgress);
            int failedJobCount = _sut.GetJobCountByStatus(JobStatuses.Failed);
            int doneJobCount = _sut.GetJobCountByStatus(JobStatuses.Done);

            Assert.Equal(expected: 0, waitingJobCount);
            Assert.Equal(expected: 0, inProgressJobCount);
            Assert.Equal(expected: 1, failedJobCount);
            Assert.Equal(expected: 0, doneJobCount);
        }

        [Fact]
        public void WhenThereIsMultipleJobInStorage_and_NoneOfThemIsFailed__FailedJobCountShouldBe0()
        {
            var message = new Message(null);
            var job1 = new Job(message, "handler-name");
            var job2 = new Job(message, "handler-name");
            job1.SetDone("test");
            _sut.Add(message, new[] {job1, job2});


            int waitingJobCount = _sut.GetJobCountByStatus(JobStatuses.Waiting);
            int inProgressJobCount = _sut.GetJobCountByStatus(JobStatuses.InProgress);
            int failedJobCount = _sut.GetJobCountByStatus(JobStatuses.Failed);
            int doneJobCount = _sut.GetJobCountByStatus(JobStatuses.Done);

            Assert.Equal(expected: 1, waitingJobCount);
            Assert.Equal(expected: 0, inProgressJobCount);
            Assert.Equal(expected: 0, failedJobCount);
            Assert.Equal(expected: 1, doneJobCount);
        }
    }
}