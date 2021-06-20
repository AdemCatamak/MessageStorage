using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.Postgres.DbClient
{
    public interface IPostgresMessageStorageTransaction : IMessageStorageTransaction
    {
        public new IPostgresMessageStorageConnection Connection { get; }
        Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken);
    }
}