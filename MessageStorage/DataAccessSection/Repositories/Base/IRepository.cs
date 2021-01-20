using System;
using System.Data;
using MessageStorage.Configurations;
using MessageStorage.Models.Base;

namespace MessageStorage.DataAccessSection.Repositories.Base
{
    public interface IRepository<in TEntity> : IDisposable
        where TEntity : Entity
    {
        void Add(TEntity entity);
    }
    
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> 
        where TEntity : Entity
    {
        protected readonly IDbConnection DbConnection;
        protected readonly IDbTransaction? DbTransaction;
        protected readonly MessageStorageRepositoryContextConfiguration MessageStorageRepositoryContextConfiguration;

        protected BaseRepository(IDbTransaction dbTransaction, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : this(dbTransaction.Connection, dbTransaction, messageStorageRepositoryContextConfiguration)
        {
        }

        protected BaseRepository(IDbConnection dbConnection, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : this(dbConnection, null, messageStorageRepositoryContextConfiguration)
        {
        }

        private BaseRepository(IDbConnection dbConnection, IDbTransaction? dbTransaction, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            DbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            DbTransaction = dbTransaction;
            MessageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration ?? throw new ArgumentNullException(nameof(messageStorageRepositoryContextConfiguration));
        }

        public abstract void Add(TEntity entity);

        public void Dispose()
        {
            // This class didn't create anything so it has no right to dispose anything
        }
    }
}