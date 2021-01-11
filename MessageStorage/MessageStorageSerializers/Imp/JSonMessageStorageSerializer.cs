using Newtonsoft.Json;

namespace MessageStorage.MessageStorageSerializers.Imp
{
    internal class JSonMessageStorageSerializer : IMessageStorageSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
                                                                      {
                                                                          TypeNameHandling = TypeNameHandling.All,
                                                                          DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                                          FloatFormatHandling = FloatFormatHandling.DefaultValue,
                                                                          TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                                                                      };

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _serializerSettings);
        }

        public T Deserialize<T>(string serializedObj)
        {
            return JsonConvert.DeserializeObject<T>(serializedObj, _serializerSettings);
        }
    }
}