using MessageStorage.MessageStorageSerializers;
using MessageStorage.MessageStorageSerializers.Imp;

namespace MessageStorage
{
    public static class MessageStorageSerializer
    {
        public static IMessageStorageSerializer OfficeSerializer { get; set; }

        static MessageStorageSerializer()
        {
            OfficeSerializer = new JSonMessageStorageSerializer();
        }


        public static string Serialize(object obj)
        {
            return OfficeSerializer.Serialize(obj);
        }


        public static T Deserialize<T>(string serializedObj)
        {
            return OfficeSerializer.Deserialize<T>(serializedObj);
        }
    }
}