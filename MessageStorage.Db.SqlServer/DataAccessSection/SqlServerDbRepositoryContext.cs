using System.Data;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Imp;
using MessageStorage.Db.DataAccessSection.Repositories;
using MessageStorage.Db.SqlServer.DataAccessSection.Repositories;

namespace MessageStorage.Db.SqlServer.DataAccessSection
{
    public class SqlServerDbRepositoryContext<TDbRepositoryConfiguration>
        : DbRepositoryContext<TDbRepositoryConfiguration>
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        public SqlServerDbRepositoryContext(TDbRepositoryConfiguration dbRepositoryConfiguration, ISqlServerDbConnectionFactory dbConnectionFactory) : base(dbRepositoryConfiguration, dbConnectionFactory)
        {
        }

        protected override IDbMessageRepository<TDbRepositoryConfiguration> CreateMessageRepository(IDbConnection dbConnection)
        {
            return new SqlServerDbMessageRepository<TDbRepositoryConfiguration>(dbConnection, DbRepositoryConfiguration);
        }

        protected override IDbJobRepository<TDbRepositoryConfiguration> CreateJobRepository(IDbConnection dbConnection)
        {
            return new SqlServerDbJobRepository<TDbRepositoryConfiguration>(dbConnection, DbRepositoryConfiguration);
        }
    }
}