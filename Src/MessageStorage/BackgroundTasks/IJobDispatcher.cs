using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.BackgroundTasks
{
    public interface IJobDispatcher
    {
        Task<bool> HandleNextJobAsync(CancellationToken cancellationToken = default);
    }
}