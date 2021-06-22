using System;

namespace MessageStorage.DataAccessLayer
{
    public class RepositoryConfiguration
    {
        public string ConnectionString { get; }
        public string Schema { get; }

        public RepositoryConfiguration(string connectionString, string? schema = null)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            
            schema = schema?.Trim() ?? string.Empty;
            Schema = schema == string.Empty ? "message_storage" : schema;
        }
    }
}