using System;
using System.Data;
using System.Data.SqlClient;

namespace MessageStorage.Db.MsSql
{
    public class MsSqlDbConnectionFactory : IMsSqlDbConnectionFactory
    {
        public MessageStorageDbConfiguration MessageStorageDbConfiguration { get; private set; }

        public void SetConfiguration(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            MessageStorageDbConfiguration = messageStorageDbConfiguration ?? throw new ArgumentNullException(nameof(messageStorageDbConfiguration));
        }

        public IDbConnection CreateConnection()
        {
            var sqlConnection = new SqlConnection(MessageStorageDbConfiguration.ConnectionStr);
            return sqlConnection;
        }
    }
}