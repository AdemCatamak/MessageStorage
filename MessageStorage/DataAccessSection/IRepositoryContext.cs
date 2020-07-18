using System;
using MessageStorage.DataAccessSection.Repositories;

namespace MessageStorage.DataAccessSection
{
    public interface IRepositoryContext : IDisposable
    {
        IMessageRepository MessageRepository { get; }
        IJobRepository JobRepository { get; }
    }
}