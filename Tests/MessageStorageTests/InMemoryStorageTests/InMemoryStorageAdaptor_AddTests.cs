using MessageStorage;
using MessageStorage.Exceptions;
using Xunit;

namespace MessageStorageTests.InMemoryStorageTests
{
    public class InMemoryStorageAdaptor_AddTests
    {
        private readonly InMemoryStorageAdaptor _sut;

        public InMemoryStorageAdaptor_AddTests()
        {
            _sut = new InMemoryStorageAdaptor();
        }

        [Fact]
        public void WhenAddOperationExecutedWithoutMessage__MessageNullExceptionOccurs()
        {
            Assert.Throws<MessageNullException>(() => _sut.Add(null, null));
        }

        [Fact]
        public void WhenAddOperationExecutedWithoutJobCollection__MessageIdShouldChange()
        {
            var message = new Message(new SomeEntity());

            _sut.Add(message, null);

            Assert.NotNull(message);
            Assert.Equal(1, message.Id);
        }

        [Fact]
        public void WhenAddOperationExecutedSuccessfully__MessageIdShouldChange_and_JobIdShouldChange()
        {
            var message = new Message(new SomeEntity());
            var job = new Job(message, "assigned-handler");

            _sut.Add(message, new[] {job});

            Assert.NotNull(message);
            Assert.Equal(1, message.Id);

            Assert.NotNull(job);
            Assert.Equal(1, job.Id);
            Assert.Equal(1, job.MessageId);
        }

        [Fact]
        public void WhenAddOperationExecutedSuccessfully__LastMessageIdShouldBeGreatestOne()
        {
            var firstMessage = new Message(new SomeEntity());
            var firstJob = new Job(firstMessage, "assigned-handler");
            _sut.Add(firstMessage, new[] {firstJob});

            var secondMessage = new Message(new SomeEntity());
            var secondJob = new Job(secondMessage, "assigned-handler");

            _sut.Add(secondMessage, new[] {secondJob});

            Assert.NotNull(firstMessage);
            Assert.Equal(1, firstMessage.Id);

            Assert.NotNull(secondMessage);
            Assert.Equal(2, secondMessage.Id);

            Assert.NotNull(firstJob);
            Assert.Equal(1, firstJob.Id);
            Assert.Equal(1, firstJob.MessageId);

            Assert.NotNull(secondJob);
            Assert.Equal(2, secondJob.Id);
            Assert.Equal(2, secondJob.MessageId);
        }

        private class SomeEntity
        {
        }
    }
}