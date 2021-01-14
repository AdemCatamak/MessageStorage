using System;
using System.Data;

namespace MessageStorage.DataAccessSection
{
    public interface IMessageStorageTransaction : IDbTransaction
    {
        event EventHandler? Committed;
        event EventHandler? Rollbacked;

        IDbTransaction ToDbTransaction();
    }

    public class MessageStorageTransaction : IMessageStorageTransaction
    {
        private readonly IDbTransaction _dbTransaction;

        public MessageStorageTransaction(IDbTransaction dbTransaction, EventHandler? committed = null, EventHandler? rollbacked = null)
        {
            _dbTransaction = dbTransaction ?? throw new ArgumentNullException(nameof(dbTransaction));
            Committed = committed;
            Rollbacked = rollbacked;
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
        }

        public IDbTransaction ToDbTransaction()
        {
            return _dbTransaction;
        }

        public event EventHandler? Committed;
        public event EventHandler? Rollbacked;
    }
}