using MessageStorage.Extensions;
using MessageStorage.Processor;
using MessageStorage.SqlServer.DataAccessLayer;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.SqlServer.Extensions;

public static class MessageStorageOptionsExtension
{
    public static MessageStorageOptionsFor<TMessageStorageClient> UseSqlServer<TMessageStorageClient>(this MessageStorageOptionsFor<TMessageStorageClient> messageStorageOptions, string connectionString, string? schema = null)
        where TMessageStorageClient : IMessageStorageClient
    {
        var repositoryContextConfiguration = new SqlServerRepositoryContextConfiguration(connectionString, schema);

        messageStorageOptions.RegisterRepositoryContextFor(() => new SqlServerStorageInitializeEngine(repositoryContextConfiguration),
                                                           provider => new SqlServerRepositoryContextFor<TMessageStorageClient>(repositoryContextConfiguration,
                                                                                                                                provider.GetRequiredService<IJobQueueFor<TMessageStorageClient>>()));

        return messageStorageOptions;
    }
}