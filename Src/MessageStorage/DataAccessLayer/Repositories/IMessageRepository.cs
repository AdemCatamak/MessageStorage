using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.DataAccessLayer.Repositories
{
    public interface IMessageRepository
    {
        Task AddAsync(Message message, CancellationToken cancellationToken = default);
    }
}