using System;

namespace MessageStorage.Db
{
    public class MessageStorageDbConfigurationFactory
    {
        public static MessageStorageDbConfiguration Create(string connectionStr, string schema = null)
        {
            var messageStorageDbConfiguration = new MessageStorageDbConfiguration(connectionStr, schema);
            return messageStorageDbConfiguration;
        }
    }

    public class MessageStorageDbConfiguration
    {
        public string Schema { get; private set; } = "MessageStorage";
        public string ConnectionStr { get; private set; }

        internal MessageStorageDbConfiguration(string connectionStr, string schema = null)
        {
            ConnectionStr = connectionStr ?? throw new ArgumentNullException(nameof(connectionStr));
            schema = schema?.Trim();
            Schema = string.IsNullOrEmpty(schema) ? Schema : schema;
        }
    }
}