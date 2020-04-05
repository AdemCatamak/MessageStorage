using System;

namespace MessageStorage.Db
{
    [Obsolete("This class will be deleted at package's version 2.0. You can create your own class based on MessageStorageDbConfiguration")]
    public class MessageStorageDbConfigurationFactory
    {
        private sealed class DefaultMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            public DefaultMessageStorageDbConfiguration(string connectionStr, string schema = null)
            {
                ConnectionStr = connectionStr;
                Schema = schema;
            }

            public override string ConnectionStr { get; protected set; }
        }

        public static MessageStorageDbConfiguration Create(string connectionStr, string schema = null)
        {
            var messageStorageDbConfiguration = new DefaultMessageStorageDbConfiguration(connectionStr, schema);
            return messageStorageDbConfiguration;
        }
    }

    public abstract class MessageStorageDbConfiguration
    {
        public string Schema { get; protected set; } = "MessageStorage";
        public abstract string ConnectionStr { get; protected set; }
    }
}