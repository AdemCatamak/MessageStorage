using MessageStorage.MessageStorageSerializers;
using MessageStorage.MessageStorageSerializers.Imp;

namespace MessageStorage
{
    public static class MessageStorageSerializer
    {
        public static IMessageStorageSerializer Serializer { get; set; }

        static MessageStorageSerializer()
        {
            Serializer = new JSonMessageStorageSerializer();
        }


        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }


        public static T Deserialize<T>(string serializedObj)
        {
            return Serializer.Deserialize<T>(serializedObj);
        }
    }
}