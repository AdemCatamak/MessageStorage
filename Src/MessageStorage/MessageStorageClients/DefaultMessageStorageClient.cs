using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.Processor;

namespace MessageStorage.MessageStorageClients;

public class DefaultMessageStorageClient : BaseMessageStorageClient<DefaultMessageStorageClient>
{
    public DefaultMessageStorageClient(IRepositoryContextFor<DefaultMessageStorageClient> repositoryContext, IMessageHandlerMetaDataHolderFor<DefaultMessageStorageClient> metaDataHolder)
        : base(repositoryContext, metaDataHolder)
    {
    }
}