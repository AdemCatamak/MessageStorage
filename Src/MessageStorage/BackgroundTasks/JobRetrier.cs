using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks.Options;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;

namespace MessageStorage.BackgroundTasks
{
    public class JobRetrier : IJobRetrier
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public JobRetrier(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task RetryAsync(RetryOption retryOption, CancellationToken cancellationToken = default)
        {
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.SetInQueued(retryOption.MessageHandlerMetadata.MessageHandlerTypeName,
                                            retryOption.RetryCount + 1,
                                            retryOption.DeferTime,
                                            cancellationToken);
        }
    }
}