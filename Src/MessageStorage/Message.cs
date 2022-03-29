using System;
using MassTransit;

namespace MessageStorage;

public class Message
{
    public Guid Id { get; private set; }
    public object Payload { get; private set; }
    public DateTime CreatedOn { get; private set; }

    public Message(object payload)
        : this(NewId.Next().ToSequentialGuid(), payload, DateTime.UtcNow)
    {
    }

    internal Message(Guid id, object payload, DateTime createdOn)
    {
        Id = id;
        Payload = payload;
        CreatedOn = createdOn;
    }
}