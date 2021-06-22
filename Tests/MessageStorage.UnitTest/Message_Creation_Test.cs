using System;
using System.Collections.Generic;
using System.Threading;
using TestUtility;
using Xunit;

namespace MessageStorage.UnitTest
{
    public class Message_Creation_Test
    {
        [Fact]
        public void When_MessageCreated__MessageIdShouldNotBeNull()
        {
            Message message = new Message("some-payload");

            Assert.NotEmpty(message.Id.ToString());
        }

        [Fact]
        public void When_MessageCreatedOneAfterAnother__MessageIdShouldBeGreaterThanPreviousOne()
        {
            List<Message> messages = new List<Message>();
            for (var i = 0; i < 100; i++)
            {
                messages.Add(new Message(i));
                Thread.Sleep(1);
            }

            for (var i = 1; i < messages.Count; i++)
            {
                Message message = messages[i];
                Message messagePrevious = messages[i - 1];

                AssertThat.LessThan((int) message.Payload, (int) messagePrevious.Payload);
                AssertThat.LessThan(message.CreatedOn, messagePrevious.CreatedOn);
                AssertThat.LessThan(message.Id, messagePrevious.Id);
            }
        }

        [Fact]
        public void When_MessageCreatedWithFullProperty__MessageContainsAllProperty()
        {
            var guid = Guid.NewGuid();
            var payload = "1";
            DateTime createdOn = DateTime.Parse("1923-10-29");

            Message message = new Message(guid, payload, createdOn);

            Assert.Equal(guid, message.Id);
            Assert.Equal(payload, message.Payload);
            Assert.Equal(createdOn, message.CreatedOn);
        }
    }
}