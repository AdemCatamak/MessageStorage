using FluentMigrator.Runner.VersionTableInfo;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

public class M0001VersionTable : IVersionTableMetaData
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public string SchemaName => _repositoryContextConfiguration.Schema ?? string.Empty;
    public string TableName => "VersionInfo";
    public string ColumnName => "Version";
    public string UniqueIndexName => "UC_Version";
    public string AppliedOnColumnName => "AppliedOn";
    public string DescriptionColumnName => "Description";
    public bool OwnsSchema => true;
    public object ApplicationContext { get; set; } = null!;

    public M0001VersionTable(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }
}