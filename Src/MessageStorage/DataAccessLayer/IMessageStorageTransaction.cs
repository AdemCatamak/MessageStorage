using System;

namespace MessageStorage.DataAccessLayer
{
    public interface IMessageStorageTransaction : IDisposable
    {
        IMessageStorageConnection Connection { get; }
        void Commit();
    }
}