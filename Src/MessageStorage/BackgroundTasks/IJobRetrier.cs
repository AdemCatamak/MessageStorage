using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers.Options;

namespace MessageStorage.BackgroundTasks
{
    public interface IJobRetrier
    {
        Task RetryAsync(RetryOption retryOption, CancellationToken cancellationToken = default);
    }
}