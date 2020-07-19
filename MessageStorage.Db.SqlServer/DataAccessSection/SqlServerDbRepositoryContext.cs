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

        protected override IMessageDbRepository CreateMessageRepository(IDbConnection dbConnection)
        {
            return new SqlServerMessageDbRepository(dbConnection, DbRepositoryConfiguration);
        }

        protected override IJobDbRepository CreateJobRepository(IDbConnection dbConnection)
        {
            return new SqlServerJobDbRepository(dbConnection, DbRepositoryConfiguration);
        }
    }
}