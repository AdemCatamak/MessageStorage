using System.Data;

namespace SampleWebApi_UseTransaction.DataAccess;

public interface IConnectionFactory
{
    IDbConnection CreateConnection();
}