using System;
using System.Data;

namespace MessageStorage.DataAccessSection
{
    public interface IMessageStorageTransaction : IDbTransaction
    {
        event EventHandler? Committed;
        event EventHandler? Rollbacked;
        event EventHandler? Disposed;

        IDbTransaction ToDbTransaction();
    }

    public class MessageStorageTransaction : IMessageStorageTransaction
    {
        private IDbTransaction _dbTransaction;

        public MessageStorageTransaction(IDbTransaction dbTransaction, EventHandler? committed = null, EventHandler? rollbacked = null, EventHandler? disposed = null)
        {
            _dbTransaction = dbTransaction ?? throw new ArgumentNullException(nameof(dbTransaction));
            Committed = committed;
            Rollbacked = rollbacked;
            Disposed = disposed;
        }

        public IDbConnection Connection => _dbTransaction.Connection;
        public IsolationLevel IsolationLevel => _dbTransaction.IsolationLevel;

        public void Commit()
        {
            _dbTransaction.Commit();
            EventHandler? handler = Committed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Rollback()
        {
            _dbTransaction.Rollback();
            EventHandler? handler = Rollbacked;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _dbTransaction.Dispose();
            _dbTransaction = null!;
            EventHandler? handler = Disposed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public IDbTransaction ToDbTransaction()
        {
            return _dbTransaction;
        }

        public event EventHandler? Committed;
        public event EventHandler? Rollbacked;
        public event EventHandler? Disposed;
    }
}