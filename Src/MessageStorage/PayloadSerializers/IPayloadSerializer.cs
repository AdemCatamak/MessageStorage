namespace MessageStorage.PayloadSerializers
{
    public interface IPayloadSerializer
    {
        string Serialize(object payload);
        object? Deserialize(string payloadStr);
    }
}