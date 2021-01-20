using System;

namespace MessageStorage.Configurations
{
    public class MessageStorageRepositoryContextConfiguration
    {
        public string ConnectionStr { get; private set; }
        public string Schema { get; private set; } = "MessageStorage";

        public MessageStorageRepositoryContextConfiguration(string connectionStr, string? schema = null)
        {
            ConnectionStr = connectionStr ?? throw new ArgumentNullException(nameof(connectionStr));
            Schema = schema ?? Schema;
        }
    }
}