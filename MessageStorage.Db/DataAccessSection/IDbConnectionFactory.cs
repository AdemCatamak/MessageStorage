using System.Data;

namespace MessageStorage.Db.DataAccessSection
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection(string dbConnectionStr);
    }
}