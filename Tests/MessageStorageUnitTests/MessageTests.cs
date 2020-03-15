using System;
using MessageStorage;
using Newtonsoft.Json;
using Xunit;

namespace MessageStorageUnitTests
{
    public class MessageTests
    {
        [Fact]
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
            Assert.Equal(traceId, message.TraceId);
            Assert.Equal(0, message.MessageId);
            Assert.True(functionStartDate <= message.CreatedOn);
            Assert.NotNull(message.PayloadClassName);
            Assert.Equal(nameof(SomeEntityCreatedEvent), message.PayloadClassName);
            Assert.NotNull(message.PayloadClassNamespace);
            Assert.Equal(typeof(SomeEntityCreatedEvent).Namespace, message.PayloadClassNamespace);
            Assert.NotNull(message.Payload);
            Assert.True(message.Payload is SomeEntityCreatedEvent);
            Assert.NotNull(message.SerializedPayload);
            Assert.Equal(1, JsonConvert.DeserializeObject<SomeEntityCreatedEvent>(message.SerializedPayload).Id);
        }

        [Fact]
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
            Assert.Equal(traceId, message.TraceId);
            Assert.Equal(id, message.MessageId);
            Assert.Equal(createOn, message.CreatedOn);
            Assert.NotNull(message.PayloadClassName);
            Assert.Equal(nameof(SomeEntityCreatedEvent), message.PayloadClassName);
            Assert.NotNull(message.PayloadClassNamespace);
            Assert.Equal(typeof(SomeEntityCreatedEvent).Namespace, message.PayloadClassNamespace);
            Assert.NotNull(message.Payload);
            Assert.True(message.Payload is SomeEntityCreatedEvent);
            Assert.True(message.Payload is SomeEntityCreatedEvent entityCreatedEvent && entityCreatedEvent.Id == 4);
            Assert.NotNull(message.SerializedPayload);
            Assert.Equal(4, JsonConvert.DeserializeObject<SomeEntityCreatedEvent>(message.SerializedPayload).Id);
        }

        [Fact]
        void WhenSetIdMethodIsUsed__IdVariableShouldBeChanged()
        {
            var someEntityCreatedEvent = new SomeEntityCreatedEvent();
            var message = new Message(someEntityCreatedEvent);

            Assert.Equal(0, message.MessageId);

            message.SetId(4);

            Assert.Equal(4, message.MessageId);
        }

        [Fact]
        public void WhenMessageObjectSerializedPayloadIsNull__PayloadObjectShouldBeNull()
        {
            var message = new Message(3, default, null, null, null, null);

            Assert.NotNull(message);
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