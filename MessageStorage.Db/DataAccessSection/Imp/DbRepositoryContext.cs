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
        private IMessageDbRepository _messageDbRepository;
        private IJobDbRepository _jobDbRepository;

        public IMessageRepository MessageRepository => MessageDbRepository;
        public IJobRepository JobRepository => JobDbRepository;

        public IMessageDbRepository MessageDbRepository
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_messageDbRepository == null)
                {
                    IDbConnection dbConnection = GetDbConnection();
                    _messageDbRepository = CreateMessageRepository(dbConnection);
                    _messageDbRepository.UseTransaction(_dbTransaction);
                }

                return _messageDbRepository;
            }
        }

        public IJobDbRepository JobDbRepository
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_jobDbRepository == null)
                {
                    IDbConnection dbConnection = GetDbConnection();
                    _jobDbRepository = CreateJobRepository(dbConnection);
                    _jobDbRepository.UseTransaction(_dbTransaction);
                }

                return _jobDbRepository;
            }
        }

        public DbRepositoryConfiguration DbRepositoryConfiguration { get; }
        public bool HasTransaction => _dbTransaction != null;

        protected DbRepositoryContext(DbRepositoryConfiguration dbRepositoryConfiguration, IDbConnectionFactory dbConnectionFactory)
        {
            DbRepositoryConfiguration = dbRepositoryConfiguration;
            _dbConnectionFactory = dbConnectionFactory;
        }

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
            _messageDbRepository?.UseTransaction(_dbTransaction);
            _jobDbRepository?.UseTransaction(_dbTransaction);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ClearTransaction()
        {
            _dbTransaction = null;
            _messageDbRepository?.ClearTransaction();
            _jobDbRepository?.ClearTransaction();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private IDbConnection GetDbConnection()
        {
            return _dbConnection ??= _dbConnectionFactory.CreateConnection(DbRepositoryConfiguration.ConnectionString);
        }

        protected abstract IMessageDbRepository CreateMessageRepository(IDbConnection dbConnection);
        protected abstract IJobDbRepository CreateJobRepository(IDbConnection dbConnection);

        public void Dispose()
        {
            _dbConnection?.Dispose();
            _dbConnection = null;
        }
    }
}