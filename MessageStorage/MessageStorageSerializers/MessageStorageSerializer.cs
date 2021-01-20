using System;
using MessageStorage.MessageStorageSerializers.Imp;

namespace MessageStorage.MessageStorageSerializers
{
    internal static class MessageStorageSerializer
    {
        public static IMessageStorageSerializer Serializer { get; private set; }

        static MessageStorageSerializer()
        {
            Serializer = new JSonMessageStorageSerializer();
        }

        public static void SetMessageStorageSerializer(IMessageStorageSerializer messageStorageSerializer)
        {
            Serializer = messageStorageSerializer ?? throw new ArgumentNullException(nameof(messageStorageSerializer));
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