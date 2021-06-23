using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;

namespace MessageStorage
{
    public class MessageStorageClient : IMessageStorageClient
    {
        private readonly IMessageHandlerProvider _messageHandlerProvider;
        private readonly IRepositoryFactory _repositoryFactory;
        private IMessageStorageTransaction? _borrowedTransaction;

        public MessageStorageClient(IMessageHandlerProvider messageHandlerProvider, IRepositoryFactory repositoryFactory)
        {
            _messageHandlerProvider = messageHandlerProvider;
            _repositoryFactory = repositoryFactory;
        }

        public void UseTransaction(IMessageStorageTransaction messageStorageTransaction)
        {
            _borrowedTransaction = messageStorageTransaction;
        }

        public async Task<(Message, IEnumerable<Job>)> AddMessageAsync(object payload, CancellationToken cancellationToken = default)
        {
            if (_borrowedTransaction == null)
            {
                using IMessageStorageConnection connection = _repositoryFactory.CreateConnection();
                using IMessageStorageTransaction transaction = connection.BeginTransaction();

                var (message, jobs) = await AddMessageAsync(payload, transaction, cancellationToken);

                transaction.Commit();

                return (message, jobs);
            }
            else
            {
                var result = await AddMessageAsync(payload, _borrowedTransaction, cancellationToken);
                return result;
            }
        }

        public async Task<(Message, IEnumerable<Job>)> AddMessageAsync(object payload, IMessageStorageTransaction messageStorageTransaction, CancellationToken cancellationToken = default)
        {
            Message message = new Message(payload);

            var compatibleMessageHandlerMetadataList = _messageHandlerProvider.GetCompatibleMessageHandler(payload.GetType())
                                                                              .ToList();
            IEnumerable<Job> jobs = compatibleMessageHandlerMetadataList.Select(metaData => new Job(message, metaData.MessageHandlerTypeName))
                                                                        .ToList();

            await AddMessageAsync(message, jobs, messageStorageTransaction, cancellationToken);

            return (message, jobs);
        }

        private async Task AddMessageAsync(Message message, IEnumerable<Job> jobs, IMessageStorageTransaction transaction, CancellationToken cancellationToken)
        {
            IMessageRepository messageRepository = _repositoryFactory.CreateMessageRepository(transaction);
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository(transaction);

            await messageRepository.AddAsync(message, cancellationToken);
            foreach (Job job in jobs)
            {
                await jobRepository.AddAsync(job, cancellationToken);
            }
        }
    }
}