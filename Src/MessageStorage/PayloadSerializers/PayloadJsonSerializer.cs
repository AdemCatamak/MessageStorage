using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MessageStorage.PayloadSerializers;

internal class PayloadJsonSerializer : IPayloadSerializer
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
                                                                      {
                                                                                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                                                                                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                                                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                                                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                                                                NullValueHandling = NullValueHandling.Ignore,
                                                                                TypeNameHandling = TypeNameHandling.All,
                                                                                FloatParseHandling = FloatParseHandling.Double,
                                                                                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
                                                                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                                            };

    public string Serialize(object payload)
    {
        string result = JsonConvert.SerializeObject(payload, _jsonSerializerSettings);
        return result;
    }

    public object? Deserialize(string payloadStr)
    {
        object? result = JsonConvert.DeserializeObject(payloadStr, _jsonSerializerSettings);
        return result;
    }
}