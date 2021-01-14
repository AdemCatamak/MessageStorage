using MessageStorage;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;

namespace AccountWebApi.SecondMessageStorageSection
{
    public interface ISecondMessageStorageClient : IMessageStorageClient
    {
    }

    public class SecondMessageStorageClient : MessageStorageClient,
                                              ISecondMessageStorageClient

    {
        public SecondMessageStorageClient(IMessageStorageRepositoryContext messageStorageRepositoryContext,
                                          IHandlerManager handlerManager,
                                          MessageStorageClientConfiguration? messageStorageConfiguration = null)
            : base(messageStorageRepositoryContext, handlerManager, messageStorageConfiguration)
        {
        }
    }
}