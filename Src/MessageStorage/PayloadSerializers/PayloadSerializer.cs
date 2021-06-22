namespace MessageStorage.PayloadSerializers
{
    public static class PayloadSerializer
    {
        public static IPayloadSerializer Serializer { get; private set; } = new PayloadJsonSerializer();

        public static void SetSerializer(IPayloadSerializer serializer)
        {
            Serializer = serializer;
        }

        public static string Serialize(object payload)
        {
            return Serializer.Serialize(payload);
        }

        public static object? Deserialize(string payloadStr)
        {
            return Serializer.Deserialize(payloadStr);
        }
    }
}