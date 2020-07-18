using System.Data;
using System.Runtime.CompilerServices;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories;
using MessageStorage.Db.Exceptions;

namespace MessageStorage.Db.DataAccessSection.Imp
{
    public abstract class DbRepositoryContext : IDbRepositoryContext
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        private IDbConnection _dbConnection;
        private IDbTransaction _dbTransaction;

        private IDbMessageRepository _dbMessageRepository;
        private IDbJobRepository _dbJobRepository;

        public IMessageRepository MessageRepository => DbMessageRepository;
        public IJobRepository JobRepository => DbJobRepository;

        public DbRepositoryConfiguration DbRepositoryConfiguration { get; }
        public bool HasTransaction => _dbTransaction != null;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            IDbTransaction dbTransaction = GetDbConnection().BeginTransaction(isolationLevel);
            UseTransaction(dbTransaction);
            return dbTransaction;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UseTransaction(IDbTransaction dbTransaction)
        {
            if (_dbTransaction != null)
            {
                throw new AnotherTransactionAssignedException();
            }

            _dbTransaction = dbTransaction;
            _dbMessageRepository?.UseTransaction(_dbTransaction);
            _dbJobRepository?.UseTransaction(_dbTransaction);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ClearTransaction()
        {
            _dbTransaction = null;
            _dbMessageRepository?.ClearTransaction();
            _dbJobRepository?.ClearTransaction();
        }

        public IDbMessageRepository DbMessageRepository
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_dbMessageRepository == null)
                {
                    IDbConnection dbConnection = GetDbConnection();
                    _dbMessageRepository = CreateMessageRepository(dbConnection);
                    _dbMessageRepository.UseTransaction(_dbTransaction);
                }

                return _dbMessageRepository;
            }
        }

        public IDbJobRepository DbJobRepository
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_dbJobRepository == null)
                {
                    IDbConnection dbConnection = GetDbConnection();
                    _dbJobRepository = CreateJobRepository(dbConnection);
                    _dbJobRepository.UseTransaction(_dbTransaction);
                }

                return _dbJobRepository;
            }
        }

        protected DbRepositoryContext(DbRepositoryConfiguration dbRepositoryConfiguration, IDbConnectionFactory dbConnectionFactory)
        {
            DbRepositoryConfiguration = dbRepositoryConfiguration;
            _dbConnectionFactory = dbConnectionFactory;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private IDbConnection GetDbConnection()
        {
            return _dbConnection ??= _dbConnectionFactory.CreateConnection(DbRepositoryConfiguration.ConnectionString);
        }

        protected abstract IDbMessageRepository CreateMessageRepository(IDbConnection dbConnection);
        protected abstract IDbJobRepository CreateJobRepository(IDbConnection dbConnection);

        public void Dispose()
        {
            _dbConnection?.Dispose();
            _dbConnection = null;
        }
    }
}