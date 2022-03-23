using FluentMigrator;

namespace MessageStorage.Postgres.DataAccessLayer.Migrations;

[Migration(6)]
public class M0006CreateIndexJobTableStatus : Migration
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0006CreateIndexJobTableStatus(PostgresRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Index("IX_Jobs_JobStatus")
              .OnTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .OnColumn("job_status");
    }

    public override void Down()
    {
        Delete.Index("IX_Jobs_JobStatus")
              .OnTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}