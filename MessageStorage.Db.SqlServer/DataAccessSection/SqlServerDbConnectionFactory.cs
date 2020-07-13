using System.Data;
using System.Data.SqlClient;
using MessageStorage.Db.DataAccessSection;

namespace MessageStorage.Db.SqlServer.DataAccessSection
{
    public interface ISqlServerDbConnectionFactory : IDbConnectionFactory
    {
    }

    public class SqlServerDbConnectionFactory : ISqlServerDbConnectionFactory
    {
        public IDbConnection CreateConnection(string dbConnectionStr)
        {
            var sqlConnection = new SqlConnection(dbConnectionStr);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}