using System;

namespace MessageStorage.DataAccessLayer
{
    public interface IMessageStorageConnection : IDisposable
    {
        IMessageStorageTransaction BeginTransaction();
    }
}