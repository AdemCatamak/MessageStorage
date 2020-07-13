namespace MessageStorage.MessageStorageSerializers
{
    public interface IMessageStorageSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string serializedObj);
    }
}