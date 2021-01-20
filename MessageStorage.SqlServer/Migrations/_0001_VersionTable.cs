using FluentMigrator.Runner.VersionTableInfo;
using MessageStorage.Configurations;

namespace MessageStorage.SqlServer.Migrations
{
    public class _0001_VersionTable : DefaultVersionTableMetaData
    {
        private readonly MessageStorageRepositoryContextConfiguration _messageStorageRepositoryContextConfiguration;

        public _0001_VersionTable(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            _messageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
        }

        public override string SchemaName => _messageStorageRepositoryContextConfiguration.Schema;
        public override string TableName => "VersionHistory";
        public override string AppliedOnColumnName => "ExecutionTime";
        public override string DescriptionColumnName => "VersionName";
        public override string ColumnName => "VersionNumber";
    }
}