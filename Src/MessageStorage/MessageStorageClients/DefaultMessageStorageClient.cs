using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.Logging;

namespace MessageStorage.MessageStorageClients;

public class DefaultMessageStorageClient : BaseMessageStorageClient<DefaultMessageStorageClient>
{
    public DefaultMessageStorageClient(IRepositoryContextFor<DefaultMessageStorageClient> repositoryContext, IMessageHandlerMetaDataHolderFor<DefaultMessageStorageClient> metaDataHolder, ILogger<DefaultMessageStorageClient> logger)
        : base(repositoryContext, metaDataHolder, logger)
    {
    }
}