using FluentMigrator.Runner.VersionTableInfo;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    public class _0001_VersionTable : DefaultVersionTableMetaData
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0001_VersionTable(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override string SchemaName => _dbRepositoryConfiguration.Schema;
        public override string TableName => "VersionHistory";
        public override string AppliedOnColumnName => "ExecutionTime";
        public override string DescriptionColumnName => "VersionName";
        public override string ColumnName => "VersionNumber";
    }
}