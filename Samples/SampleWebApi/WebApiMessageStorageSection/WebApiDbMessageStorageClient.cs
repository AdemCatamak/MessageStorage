using MessageStorage;
using MessageStorage.Db.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection;

namespace SampleWebApi.WebApiMessageStorageSection
{
    public class WebApiDbMessageStorageClient : MessageStorageDbClient<WebapiSqlServerDbRepositoryConfiguration>
    {
        public WebApiDbMessageStorageClient(IHandlerManager handlerManager, IDbRepositoryContext<WebapiSqlServerDbRepositoryConfiguration> dbRepositoryContext, MessageStorageDbConfiguration messageStorageConfiguration = null)
            : base(handlerManager, dbRepositoryContext, messageStorageConfiguration ?? new MessageStorageDbConfiguration())
        {
        }
    }
}