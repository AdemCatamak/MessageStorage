using System.Data;
using MessageStorage.Db.DataAccessSection;
using Microsoft.Data.SqlClient;

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