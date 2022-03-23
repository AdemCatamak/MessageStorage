using FluentMigrator;

namespace MessageStorage.Postgres.DataAccessLayer.Migrations;

[Migration(version: 2)]
public class M0002CreateTableJobStatuses : Migration
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0002CreateTableJobStatuses(PostgresRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Table("job_statuses")
              .InSchema(_repositoryContextConfiguration.Schema)
              .WithColumn("id").AsInt32().PrimaryKey("PK_JobStatuses_Id")
              .WithColumn("status_name").AsString().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("job_statuses")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}