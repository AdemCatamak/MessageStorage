using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db.DataAccessSection.Repositories.BaseRepository;
using MessageStorage.Models;

namespace MessageStorage.Db.DataAccessSection.Repositories
{
    public interface IMessageDbRepository : IMessageRepository, IDbRepository<Message>
    {
    }
}