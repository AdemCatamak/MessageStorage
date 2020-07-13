using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Models;

namespace MessageStorage.DataAccessSection.Repositories
{
    public interface IMessageRepository<out TRepositoryConfiguration>
        : IRepository<TRepositoryConfiguration, Message> where TRepositoryConfiguration : RepositoryConfiguration
    {
    }
}