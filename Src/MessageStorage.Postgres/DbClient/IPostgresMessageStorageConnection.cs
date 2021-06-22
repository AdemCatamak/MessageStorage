using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.Postgres.DbClient
{
    public interface IPostgresMessageStorageConnection : IMessageStorageConnection
    {
        new IPostgresMessageStorageTransaction BeginTransaction();
        IPostgresMessageStorageTransaction BeginTransaction(IsolationLevel isolationLevel);

        Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken);
        Task<IEnumerable<dynamic>> QueryAsync(string script, object? parameters, CancellationToken cancellationToken);
    }
}