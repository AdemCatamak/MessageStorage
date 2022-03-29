namespace MessageStorage.MessageHandlers;

public interface IMessageContext<out T>
{
    public string JobId { get; }
    public T Message { get; }
}

internal class MessageContext<T> : IMessageContext<T>
{
    public string JobId { get; }
    public T Message { get; }

    public MessageContext(string jobId, T message)
    {
        JobId = jobId;
        Message = message;
    }
}