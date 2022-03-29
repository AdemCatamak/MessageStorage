using FluentMigrator;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

[Migration(version: 2)]
public class M0002CreateTableJobStatuses : Migration
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0002CreateTableJobStatuses(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Table("JobStatuses")
              .InSchema(_repositoryContextConfiguration.Schema)
              .WithColumn("Id").AsInt32().PrimaryKey("PK_JobStatuses_Id")
              .WithColumn("StatusName").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("JobStatuses")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}