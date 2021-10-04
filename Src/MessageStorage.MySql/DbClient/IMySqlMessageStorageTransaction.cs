using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.MySql.DbClient
{
    public interface IMySqlMessageStorageTransaction : IMessageStorageTransaction
    {
        public new IMySqlMessageStorageConnection Connection { get; }
        Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken);
    }
}