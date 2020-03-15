using System.Data;

namespace MessageStorage.Db
{
    public interface IDbAdaptor
    {
        IDbConnection CreateConnection(string connectionStr);
        IDataAdapter CreateDataAdaptor(IDbCommand dbCommand);
    }
}