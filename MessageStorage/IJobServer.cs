using System.Threading.Tasks;

namespace MessageStorage
{
    public interface IJobServer
    {
        Task StartAsync();
        Task StopAsync();
    }
}