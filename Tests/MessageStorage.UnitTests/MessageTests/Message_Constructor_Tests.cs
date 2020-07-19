using System;
using MessageStorage.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MessageStorage.UnitTests.MessageTests
{
    public class Message_Constructor_Tests
    {
        [Test]
        public void WhenMessageObjectCreatedWithPayload__AnyFieldShouldNotBeNull()
        {
            DateTime functionStartDate = DateTime.UtcNow;
            const string traceId = "trace-id";

            var someEntity = new SomeEntityCreatedEvent
                             {
                                 Id = 1
                             };

            var message = new Message(someEntity, traceId);

            Assert.NotNull(message);

            Assert.IsNotEmpty(message.Id);
            Assert.True(functionStartDate <= message.CreatedOn);
            Assert.NotNull(message.GetPayload());
            Assert.True(message.GetPayload() is SomeEntityCreatedEvent);
            Assert.NotNull(message.SerializedPayload);
            Assert.AreEqual(expected: 1, JsonConvert.DeserializeObject<SomeEntityCreatedEvent>(message.SerializedPayload).Id);
        }


        [Test]
        public void WhenMessageObjectCreatedWithPrimitiveFields__PayloadObjectShouldNotBeNull()
        {
            var someEntityCreatedEvent = new SomeEntityCreatedEvent {Id = 4};
            const string id = "3";
            var createOn = new DateTime(year: 2020, month: 1, day: 15, hour: 20, minute: 56, second: 00);
            string serializedPayload = JsonConvert.SerializeObject(someEntityCreatedEvent, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});

            var message = new Message(id, createOn, serializedPayload);

            Assert.NotNull(message);

            Assert.AreEqual(id, message.Id);
            Assert.AreEqual(createOn, message.CreatedOn);
            Assert.NotNull(message.GetPayload());
            Assert.True(message.GetPayload() is SomeEntityCreatedEvent);
            Assert.True(message.GetPayload() is SomeEntityCreatedEvent entityCreatedEvent && entityCreatedEvent.Id == 4);
            Assert.NotNull(message.SerializedPayload);
            Assert.AreEqual(expected: 4, JsonConvert.DeserializeObject<SomeEntityCreatedEvent>(message.SerializedPayload).Id);
        }

        [Test]
        public void WhenMessageObjectSerializedPayloadIsNull__PayloadObjectShouldBeNull()
        {
            const string id = "3";
            var message = new Message(id, createdOn: default, serializedPayload: null);

            Assert.NotNull(message);
            Assert.AreEqual(id, message.Id);
            Assert.Null(message.SerializedPayload);
            Assert.Null(message.GetPayload());
        }

        private interface IEvent
        {
            public long Id { get; set; }
        }

        private class SomeEntityCreatedEvent : IEvent
        {
            public long Id { get; set; }
        }
    }
}