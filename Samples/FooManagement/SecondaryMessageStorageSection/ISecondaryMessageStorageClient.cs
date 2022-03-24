using MessageStorage;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MessageStorageClients;

namespace FooManagement.SecondaryMessageStorageSection;

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