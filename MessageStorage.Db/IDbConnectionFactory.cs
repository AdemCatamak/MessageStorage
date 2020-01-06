using System.Data;

namespace MessageStorage.Db
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}