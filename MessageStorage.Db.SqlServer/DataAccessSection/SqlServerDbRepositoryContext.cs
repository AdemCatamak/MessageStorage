using System.Data;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Imp;
using MessageStorage.Db.DataAccessSection.Repositories;
using MessageStorage.Db.SqlServer.DataAccessSection.Repositories;

namespace MessageStorage.Db.SqlServer.DataAccessSection
{
    public class SqlServerDbRepositoryContext : DbRepositoryContext
    {
        public SqlServerDbRepositoryContext(DbRepositoryConfiguration dbRepositoryConfiguration, ISqlServerDbConnectionFactory dbConnectionFactory)
            : base(dbRepositoryConfiguration, dbConnectionFactory)
        {
        }

        protected override IDbMessageRepository CreateMessageRepository(IDbConnection dbConnection)
        {
            return new SqlServerDbMessageRepository(dbConnection, DbRepositoryConfiguration);
        }

        protected override IDbJobRepository CreateJobRepository(IDbConnection dbConnection)
        {
            return new SqlServerDbJobRepository(dbConnection, DbRepositoryConfiguration);
        }
    }
}