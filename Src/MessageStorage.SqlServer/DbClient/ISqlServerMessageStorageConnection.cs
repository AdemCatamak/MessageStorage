using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.SqlServer.DbClient
{
    public interface ISqlServerMessageStorageConnection : IMessageStorageConnection
    {
        Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken);
        Task<IEnumerable<dynamic>> QueryAsync(string script, object? parameters, CancellationToken cancellationToken);
    }
}