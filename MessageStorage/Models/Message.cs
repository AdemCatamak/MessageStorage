using System;
using MassTransit;
using MessageStorage.Models.Base;

namespace MessageStorage.Models
{
    public class Message : Entity
    {
        public DateTime CreatedOn { get; }
        public string SerializedPayload { get; }
        
        public Message(object payload, string id = null)
            : this(id, DateTime.UtcNow, MessageStorageSerializer.Serialize(payload))
        {
        }

        public Message(string id, DateTime createdOn, string serializedPayload)
        {
            Id = id ?? NewId.Next().ToString();
            SerializedPayload = serializedPayload;
            CreatedOn = createdOn;
        }
        
        private object _payload;
        public object GetPayload()
        {
            return _payload ??= SerializedPayload == null
                                    ? null
                                    : MessageStorageSerializer.Deserialize<object>(SerializedPayload);
        }
    }
}