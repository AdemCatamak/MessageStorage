using System;
using MessageStorage;
using Newtonsoft.Json;
using NUnit.Framework;

namespace UnitTest.MessageStorage.MessageTests
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

            Assert.NotNull(message.TraceId);
            Assert.AreEqual(traceId, message.TraceId);
            Assert.AreEqual(0,message.Id);
            Assert.True(functionStartDate <= message.CreatedOn);
            Assert.NotNull(message.PayloadClassName);
            Assert.AreEqual(nameof(SomeEntityCreatedEvent), message.PayloadClassName);
            Assert.NotNull(message.PayloadClassNamespace);
            Assert.AreEqual(typeof(SomeEntityCreatedEvent).Namespace, message.PayloadClassNamespace);
            Assert.NotNull(message.Payload);
            Assert.True(message.Payload is SomeEntityCreatedEvent);
            Assert.NotNull(message.SerializedPayload);
            Assert.AreEqual(1, JsonConvert.DeserializeObject<SomeEntityCreatedEvent>(message.SerializedPayload).Id);
        }


        [Test]
        public void WhenMessageObjectCreatedWithPrimitiveFields__PayloadObjectShouldNotBeNull()
        {
            var someEntityCreatedEvent = new SomeEntityCreatedEvent {Id = 4};
            const long id = 3;
            var createOn = new DateTime(2020, 1, 15, 20, 56, 00);
            const string traceId = "trace-id";
            string payloadClassNamespace = typeof(SomeEntityCreatedEvent).Namespace;
            string payloadClassName = nameof(SomeEntityCreatedEvent);
            string serializedPayload = JsonConvert.SerializeObject(someEntityCreatedEvent, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});

            var message = new Message(id, createOn, traceId, payloadClassName, payloadClassNamespace, serializedPayload);

            Assert.NotNull(message);

            Assert.NotNull(message.TraceId);
            Assert.AreEqual(traceId, message.TraceId);
            Assert.AreEqual(id, message.Id);
            Assert.AreEqual(createOn, message.CreatedOn);
            Assert.NotNull(message.PayloadClassName);
            Assert.AreEqual(nameof(SomeEntityCreatedEvent), message.PayloadClassName);
            Assert.NotNull(message.PayloadClassNamespace);
            Assert.AreEqual(typeof(SomeEntityCreatedEvent).Namespace, message.PayloadClassNamespace);
            Assert.NotNull(message.Payload);
            Assert.True(message.Payload is SomeEntityCreatedEvent);
            Assert.True(message.Payload is SomeEntityCreatedEvent entityCreatedEvent && entityCreatedEvent.Id == 4);
            Assert.NotNull(message.SerializedPayload);
            Assert.AreEqual(4, JsonConvert.DeserializeObject<SomeEntityCreatedEvent>(message.SerializedPayload).Id);
        }

        [Test]
        public void WhenMessageObjectSerializedPayloadIsNull__PayloadObjectShouldBeNull()
        {
            const long id = 3;
            var message = new Message(id, default, null, null, null, null);

            Assert.NotNull(message);
            Assert.AreEqual(id, message.Id);
            Assert.Null(message.SerializedPayload);
            Assert.Null(message.Payload);
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