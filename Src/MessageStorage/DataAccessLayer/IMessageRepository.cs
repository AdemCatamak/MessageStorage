using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.DataAccessLayer;

public interface IMessageRepository
{
    Task AddAsync(Message message, CancellationToken cancellationToken);
    Task CleanAsync(DateTime createdBeforeThen, CancellationToken cancellationToken);
}