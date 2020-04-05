using System.Data;
using MessageStorage.Exceptions;
using Microsoft.Data.SqlClient;

namespace MessageStorage.Db.MsSql
{
    public interface IMsSqlDbAdaptor : IDbAdaptor
    {
    }

    public class MsSqlDbAdaptor : IMsSqlDbAdaptor
    {
        public IDbConnection CreateConnection(string connectionStr)
        {
            var dbConnection = new SqlConnection(connectionStr);
            return dbConnection;
        }

        public IDataAdapter CreateDataAdaptor(IDbCommand dbCommand)
        {
            if (!(dbCommand is SqlCommand sqlCommand))
            {
                throw new ArgumentNotCompatibleException(typeof(System.Data.SqlClient.SqlCommand), dbCommand.GetType());
            }

            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            return sqlDataAdapter;
        }
    }
}