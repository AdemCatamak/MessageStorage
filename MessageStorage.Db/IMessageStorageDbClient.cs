using System.Data;

namespace MessageStorage.Db
{
    public interface IMessageStorageDbClient : IMessageStorageClient
    {
        void Add<T>(T payload, IDbTransaction dbTransaction, string traceId = null, bool autoJobCreator = true);
    }
}