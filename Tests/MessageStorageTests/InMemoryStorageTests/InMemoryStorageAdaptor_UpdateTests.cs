using System;
using MessageStorage;
using MessageStorage.Exceptions;
using Xunit;

namespace MessageStorageTests.InMemoryStorageTests
{
    // ReSharper disable once InconsistentNaming
    public class InMemoryStorageAdaptor_UpdateTests
    {
        private readonly InMemoryStorageAdaptor _sut;

        public InMemoryStorageAdaptor_UpdateTests()
        {
            _sut = new InMemoryStorageAdaptor();
        }

        [Fact]
        public void WhenJobDoesNotExist__JobNotFoundExceptionOccurs()
        {
            var message = new Message(null);
            var job = new Job(message, "assigned-handler");

            var jobNotFoundException = Assert.Throws<JobNotFoundException>(() => _sut.Update(job));

            Assert.Contains(job.Id.ToString(), jobNotFoundException.Message);
        }

        [Fact]
        public void WhenJobFound__OperationSuccessfullyCompleted()
        {
            var message = new Message(null);
            var job = new Job(message, "assigned-handler");
            _sut.Add(message, new[] {job});

            var newJob = new Job(job.Id, job.Message, "new-assigned-handler", JobStatuses.Done, DateTime.UtcNow, "some-info");

            _sut.Update(newJob);
        }
    }
}