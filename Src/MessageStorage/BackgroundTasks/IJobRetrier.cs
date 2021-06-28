using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks.Options;

namespace MessageStorage.BackgroundTasks
{
    public interface IJobRetrier
    {
        Task RetryAsync(RetryOption retryOption, CancellationToken cancellationToken = default);
    }
}