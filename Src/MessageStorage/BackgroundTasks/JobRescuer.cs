using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MessageHandlers.Options;

namespace MessageStorage.BackgroundTasks
{
    public class JobRescuer : IJobRescuer
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public JobRescuer(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task RescueAsync(RescueOption rescueOption, CancellationToken cancellationToken = default)
        {
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.SetInQueued(rescueOption.MessageHandlerMetadata.MessageHandlerTypeName,
                                            rescueOption.MaxExecutionTime,
                                            cancellationToken);
        }
    }
}