using System.Threading.Tasks;

namespace MessageStorage
{
    public interface IMessageStorageClient
    {
        void Add<T>(T payload, string traceId = null, bool autoJobCreator = true);
        Job SetFirstWaitingJobToInProgress();
        Task Handle(Job job);
        void Update(Job job);
    }
}