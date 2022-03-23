using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MessageStorageClients;
using MessageStorage.Processor;

namespace MessageStorage.MultiClient.IntegrationTest.Fixtures.SecondaryMessageStorageSection;

public interface ISecondaryMessageStorageClient : IMessageStorageClient
{
}

public class SecondaryMessageStorageClient : BaseMessageStorageClient<SecondaryMessageStorageClient>,
                                             ISecondaryMessageStorageClient
{
    public SecondaryMessageStorageClient(IRepositoryContextFor<SecondaryMessageStorageClient> repositoryContext, IMessageHandlerMetaDataHolderFor<SecondaryMessageStorageClient> metaDataHolder)
        : base(repositoryContext, metaDataHolder)
    {
    }
}