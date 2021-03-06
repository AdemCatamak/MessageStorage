using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks.Options;

namespace MessageStorage.BackgroundTasks
{
    public interface IJobRescuer
    {
        Task RescueAsync(RescueOption rescueOption, CancellationToken cancellationToken = default);
    }
}