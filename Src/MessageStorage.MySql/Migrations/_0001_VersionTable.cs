using FluentMigrator.Runner.VersionTableInfo;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.MySql.Migrations
{
    public class _0001_VersionTable : IVersionTableMetaData
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;
        
        public string SchemaName => _repositoryConfiguration.Schema;
        public string TableName => "version_info";
        public string ColumnName => "version";
        public string UniqueIndexName => "uc_version";
        public string AppliedOnColumnName => "applied_on";
        public string DescriptionColumnName => "description";
        public bool OwnsSchema => true;
        public object ApplicationContext { get; set; } = null!;

        public _0001_VersionTable(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }
    }
}