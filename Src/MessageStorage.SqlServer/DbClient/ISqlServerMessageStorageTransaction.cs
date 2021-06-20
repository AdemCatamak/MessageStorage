using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.SqlServer.DbClient
{
    public interface ISqlServerMessageStorageTransaction : IMessageStorageTransaction
    {
        public new ISqlServerMessageStorageConnection Connection { get; }
        Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken);
    }
}