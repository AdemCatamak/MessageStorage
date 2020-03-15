using System;
using MassTransit;
using Newtonsoft.Json;

namespace MessageStorage
{
    public class Message
    {
        public string MessageId { get; private set; }
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
                var result = JsonConvert.DeserializeObject<object>(SerializedPayload, new JsonSerializerSettings
                                                                                      {
                                                                                          TypeNameHandling = TypeNameHandling.All,
                                                                                          DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                                                          TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                                                                                      });
                return result;
            }
            private set
            {
                SerializedPayload = JsonConvert.SerializeObject(value, new JsonSerializerSettings
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

        public Message(string messageId, DateTime createdOn, string traceId, string payloadClassName, string payloadClassNamespace, string serializedPayload)
        {
            MessageId = messageId;
            TraceId = traceId;
            PayloadClassName = payloadClassName;
            PayloadClassNamespace = payloadClassNamespace;
            SerializedPayload = serializedPayload;
            CreatedOn = createdOn;
        }

        public Message(object payload, string traceId = null)
        {
            MessageId = NewId.Next().ToString();
            TraceId = traceId;
            CreatedOn = DateTime.UtcNow;
            Payload = payload;
        }
    }
}