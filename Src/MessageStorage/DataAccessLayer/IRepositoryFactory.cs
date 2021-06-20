using MessageStorage.DataAccessLayer.Repositories;

namespace MessageStorage.DataAccessLayer
{
    public interface IRepositoryFactory
    {
        RepositoryConfiguration RepositoryConfiguration { get; }
        
        IMessageStorageConnection CreateConnection();

        IMessageRepository CreateMessageRepository();
        IMessageRepository CreateMessageRepository(IMessageStorageTransaction messageStorageTransaction);
        IJobRepository CreateJobRepository();
        IJobRepository CreateJobRepository(IMessageStorageTransaction messageStorageTransaction);
    }
}