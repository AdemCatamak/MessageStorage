using FluentMigrator.Runner.VersionTableInfo;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.SqlServer.Migrations
{
    public class _0001_VersionTable : IVersionTableMetaData
    {
        
        private readonly RepositoryConfiguration _repositoryConfiguration;
        
        public string SchemaName => _repositoryConfiguration.Schema;
        public string TableName => "VersionInfo";
        public string ColumnName => "Version";
        public string UniqueIndexName => "UC_Version";
        public string AppliedOnColumnName => "AppliedOn";
        public string DescriptionColumnName => "Description";
        public bool OwnsSchema => true;
        public object ApplicationContext { get; set; } = null!;
        
        public _0001_VersionTable(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }
    }
}