using System.Data;

namespace MessageStorage.Db
{
    public interface IDbConnectionFactory
    {
        MessageStorageDbConfiguration MessageStorageDbConfiguration { get; }
        void SetConfiguration(MessageStorageDbConfiguration messageStorageDbConfiguration);
        IDbConnection CreateConnection();
    }
}