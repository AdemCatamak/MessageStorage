using MessageStorage;
using MessageStorage.Db.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection;

namespace AccountWebApi.AccountApiMessageStorageSection
{
    public class AccountApiDbMessageStorageClient : MessageStorageDbClient<AccountApiSqlServerDbRepositoryConfiguration>
    {
        public AccountApiDbMessageStorageClient(IHandlerManager handlerManager, IDbRepositoryContext<AccountApiSqlServerDbRepositoryConfiguration> dbRepositoryContext, MessageStorageDbConfiguration messageStorageConfiguration = null)
            : base(handlerManager, dbRepositoryContext, messageStorageConfiguration ?? new MessageStorageDbConfiguration())
        {
        }
    }
}