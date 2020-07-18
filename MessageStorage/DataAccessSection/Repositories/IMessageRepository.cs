using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Models;

namespace MessageStorage.DataAccessSection.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
    }
}