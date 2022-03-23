using FluentMigrator.Runner.VersionTableInfo;

namespace MessageStorage.Postgres.DataAccessLayer.Migrations;

public class M0001VersionTable : IVersionTableMetaData
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;

    public string SchemaName => _repositoryContextConfiguration.Schema ?? string.Empty;
    public string TableName => "version_info";
    public string ColumnName => "version";
    public string UniqueIndexName => "uc_version";
    public string AppliedOnColumnName => "applied_on";
    public string DescriptionColumnName => "description";
    public bool OwnsSchema => true;
    public object ApplicationContext { get; set; } = null!;

    public M0001VersionTable(PostgresRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }
}