using MessageStorage.Clients;
using MessageStorage.Configurations;
using MessageStorage.DI.Extension;
using MessageStorage.SqlServer.DataAccessSection;

namespace MessageStorage.SqlServer.DI.Extension
{
    public static class MessageStorageDIConfigurationBuilderExtensions
    {
        public static IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseSqlServer<TMessageStorageClient>(this IMessageStorageDIConfigurationBuilder<TMessageStorageClient> builder,
                                                                                                                       string connectionStr, string? schema = null)
            where TMessageStorageClient : class, IMessageStorageClient
        {
            MessageStorageRepositoryContextConfiguration configuration = new MessageStorageRepositoryContextConfiguration(connectionStr, schema);
            return UseSqlServer(builder, configuration);
        }

        public static IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseSqlServer<TMessageStorageClient>(this IMessageStorageDIConfigurationBuilder<TMessageStorageClient> builder,
                                                                                                                       MessageStorageRepositoryContextConfiguration configuration)
            where TMessageStorageClient : class, IMessageStorageClient
        {
            builder.UseRepositoryContextConfiguration(configuration);
            builder.UseRepositoryContextFactoryMethod(contextConfiguration => new SqlServerMessageStorageRepositoryContext(contextConfiguration));
            return builder;
        }
    }
}