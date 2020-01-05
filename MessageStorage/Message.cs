using System;
using Newtonsoft.Json;

namespace MessageStorage
{
    public class Message
    {
        public Guid Id { get; private set; }
        public string TraceId { get; set; }
        public DateTime CreatedOn { get; private set; }

        public string PayloadClassName { get; private set; }
        public string PayloadClassNamespace { get; private set; }
        public string SerializedPayload { get; private set; }

        public object Payload
        {
            get
            {
                if (SerializedPayload == null)
                    return null;
                var result = JsonConvert.DeserializeObject<object>(SerializedPayload, new JsonSerializerSettings()
                                                                                      {
                                                                                          TypeNameHandling = TypeNameHandling.All,
                                                                                          DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                                                          TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                                                                                      });
                return result;
            }
            private set
            {
                SerializedPayload = JsonConvert.SerializeObject(value, new JsonSerializerSettings()
                                                                       {
                                                                           Formatting = Formatting.None,
                                                                           TypeNameHandling = TypeNameHandling.All,
                                                                           DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                                           TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                                                                       });
                PayloadClassName = value?.GetType().Name;
                PayloadClassNamespace = value?.GetType().Namespace;
            }
        }

        internal Message(Guid id, object payload, DateTime createdOn, string traceId)
        {
            Id = id;
            TraceId = traceId;
            CreatedOn = createdOn == default ? DateTime.UtcNow : createdOn;
            Payload = payload;
        }

        public Message(object payload) : this(default, payload, default, default)
        {
        }

        public Message(object payload, string traceId) : this(default, payload, default, traceId)
        {
        }
    }
}