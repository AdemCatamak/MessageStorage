using System;
using MassTransit;
using MessageStorage.MessageStorageSerializers;
using MessageStorage.Models.Base;

namespace MessageStorage.Models
{
    public class Message : Entity
    {
        public DateTime CreatedOn { get; private set; }
        public string SerializedPayload { get; private set; }

        public Message(object payload, string? id = null)
            : this(id ?? NewId.Next().ToString(),
                   DateTime.UtcNow,
                   MessageStorageSerializer.Serialize(payload))
        {
        }

        public Message(string id, DateTime createdOn, string serializedPayload)
            : base(id)
        {
            SerializedPayload = serializedPayload;
            CreatedOn = createdOn;
        }

        private object? _payload = null;

        public object GetPayload()
        {
            return _payload ??= MessageStorageSerializer.Deserialize<object>(SerializedPayload);
        }
    }
}