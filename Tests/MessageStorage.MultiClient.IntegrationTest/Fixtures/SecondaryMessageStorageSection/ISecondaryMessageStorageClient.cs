using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MessageStorageClients;
using Microsoft.Extensions.Logging;

namespace MessageStorage.MultiClient.IntegrationTest.Fixtures.SecondaryMessageStorageSection;

public interface ISecondaryMessageStorageClient : IMessageStorageClient
{
}

public class SecondaryMessageStorageClient : BaseMessageStorageClient<SecondaryMessageStorageClient>,
                                             ISecondaryMessageStorageClient
{
    public SecondaryMessageStorageClient(IRepositoryContextFor<SecondaryMessageStorageClient> repositoryContext, IMessageHandlerMetaDataHolderFor<SecondaryMessageStorageClient> metaDataHolder, ILogger<SecondaryMessageStorageClient> logger)
        : base(repositoryContext, metaDataHolder, logger)
    {
    }
}