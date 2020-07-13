using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories;

namespace MessageStorage.DataAccessSection
{
    public interface IRepositoryContext<out TRepositoryConfiguration>
        where TRepositoryConfiguration : RepositoryConfiguration
    {
        TRepositoryConfiguration RepositoryConfiguration { get; }

        IMessageRepository<TRepositoryConfiguration> MessageRepository { get; }
        IJobRepository<TRepositoryConfiguration> JobRepository { get; }
    }
}