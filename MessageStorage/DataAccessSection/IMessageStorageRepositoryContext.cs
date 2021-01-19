using System;
using System.Data;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Exceptions;

namespace MessageStorage.DataAccessSection
{
    public interface IMessageStorageRepositoryContext : IDisposable
    {
        IMessageRepository GetMessageRepository();
        IJobRepository GetJobRepository();

        IMessageStorageTransaction BeginTransaction(IsolationLevel isolationLevel);
        IMessageStorageTransaction UseTransaction(IDbTransaction dbTransaction);
        bool HasTransaction { get; }
    }

    public abstract class MessageStorageRepositoryContext : IMessageStorageRepositoryContext
    {
        protected readonly MessageStorageRepositoryContextConfiguration MessageStorageRepositoryContextConfiguration;

        private IDbConnection? _dbConnection;
        private IMessageStorageTransaction? _ownedDbTransaction;
        private IMessageStorageTransaction? _borrowedDbTransaction;

        protected IDbConnection DbConnection
        {
            get
            {
                if (_dbConnection != null) return _dbConnection;

                _dbConnection = CreateDbConnection();
                _dbConnection.Open();

                return _dbConnection;
            }
        }

        protected IMessageStorageTransaction? MessageStorageTransaction
        {
            get
            {
                if (_borrowedDbTransaction != null)
                    return _borrowedDbTransaction;

                return _ownedDbTransaction;
            }
        }

        protected MessageStorageRepositoryContext(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            MessageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration ?? throw new ArgumentNullException(nameof(messageStorageRepositoryContextConfiguration));
        }

        public bool HasTransaction => MessageStorageTransaction != null;

        public IMessageStorageTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (HasTransaction) throw new ContextAlreadyHasTransaction();

            IDbTransaction transaction = DbConnection.BeginTransaction(isolationLevel);
            var messageStorageTransaction = new MessageStorageTransaction(transaction, OwnedTransactionFinalized, OwnedTransactionFinalized, OwnedTransactionDisposed);

            _ownedDbTransaction = messageStorageTransaction;

            return messageStorageTransaction;
        }

        public IMessageStorageTransaction UseTransaction(IDbTransaction dbTransaction)
        {
            if (HasTransaction) throw new ContextAlreadyHasTransaction();

            var messageStorageTransaction = new MessageStorageTransaction(dbTransaction, BorrowedTransactionFinalized, BorrowedTransactionFinalized, BorrowedTransactionDisposed);
            _borrowedDbTransaction = messageStorageTransaction;

            return messageStorageTransaction;
        }

        public void Dispose()
        {
            // This class may create _dbConnection and _ownedDbTransaction so it has right to dispose
            _dbConnection?.Dispose();
            _ownedDbTransaction?.Dispose();
            _dbConnection = null;
            _ownedDbTransaction = null;
        }

        private void OwnedTransactionFinalized(object sender, EventArgs e)
        {
            var messageStorageTransaction = sender as IMessageStorageTransaction;
            messageStorageTransaction!.Committed -= OwnedTransactionFinalized;
            messageStorageTransaction.Rollbacked -= OwnedTransactionFinalized;
            messageStorageTransaction.Disposed -= OwnedTransactionDisposed;

            _ownedDbTransaction = null;
        }

        private void OwnedTransactionDisposed(object sender, EventArgs e)
        {
            var messageStorageTransaction = sender as IMessageStorageTransaction;
            messageStorageTransaction!.Committed -= OwnedTransactionFinalized;
            messageStorageTransaction.Rollbacked -= OwnedTransactionFinalized;
            messageStorageTransaction.Disposed -= OwnedTransactionDisposed;

            _ownedDbTransaction = null;
        }

        private void BorrowedTransactionFinalized(object sender, EventArgs e)
        {
            var messageStorageTransaction = sender as IMessageStorageTransaction;
            messageStorageTransaction!.Committed -= BorrowedTransactionFinalized;
            messageStorageTransaction.Rollbacked -= BorrowedTransactionFinalized;
            messageStorageTransaction.Disposed -= BorrowedTransactionDisposed;

            _borrowedDbTransaction = null;
        }

        private void BorrowedTransactionDisposed(object sender, EventArgs e)
        {
            var messageStorageTransaction = sender as IMessageStorageTransaction;
            messageStorageTransaction!.Committed -= BorrowedTransactionFinalized;
            messageStorageTransaction.Rollbacked -= BorrowedTransactionFinalized;
            messageStorageTransaction.Disposed -= BorrowedTransactionDisposed;

            _borrowedDbTransaction = null;
        }

        public abstract IMessageRepository GetMessageRepository();
        public abstract IJobRepository GetJobRepository();
        protected abstract IDbConnection CreateDbConnection();
    }
}