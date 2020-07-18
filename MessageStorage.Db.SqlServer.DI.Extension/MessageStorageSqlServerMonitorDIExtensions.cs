using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.SqlServer.DataAccessSection;
using MessageStorage.DI.Extension;

namespace MessageStorage.Db.SqlServer.DI.Extension
{
    public static class MessageStorageSqlServerMonitorDIExtensions
    {
        public static IMessageStorageServiceCollection AddMessageStorageSqlServerMonitor
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            return messageStorageServiceCollection.AddMessageStorageMonitor<IMessageStorageMonitor>(provider =>
                                                                                                    {
                                                                                                        var sqlServerDbConnectionFactory = new SqlServerDbConnectionFactory();
                                                                                                        var messageStorageMonitor = new MessageStorageMonitor(new SqlServerDbRepositoryContext(dbRepositoryConfiguration, sqlServerDbConnectionFactory));
                                                                                                        return messageStorageMonitor;
                                                                                                    });
        }
    }
}