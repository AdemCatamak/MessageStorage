using MessageStorage.Extensions;
using MessageStorage.Postgres.DataAccessLayer;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Postgres.Extensions;

public static class MessageStorageOptionExtension
{
    public static MessageStorageOptionsFor<TMessageStorageClient> UsePostgres<TMessageStorageClient>(this MessageStorageOptionsFor<TMessageStorageClient> messageStorageOptions, string connectionString, string? schema = null)
        where TMessageStorageClient : IMessageStorageClient
    {
        var repositoryContextConfiguration = new PostgresRepositoryContextConfiguration(connectionString, schema);

        messageStorageOptions.RegisterRepositoryContextFor(() => new PostgresStorageInitializeEngine(repositoryContextConfiguration),
                                                           provider => new PostgresRepositoryContextFor<TMessageStorageClient>(repositoryContextConfiguration,
                                                                                                                               provider.GetRequiredService<IJobQueueFor<TMessageStorageClient>>()));

        return messageStorageOptions;
    }
}