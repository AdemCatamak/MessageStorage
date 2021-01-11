namespace MessageStorage.MessageStorageSerializers
{
    internal interface IMessageStorageSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string serializedObj);
    }
}