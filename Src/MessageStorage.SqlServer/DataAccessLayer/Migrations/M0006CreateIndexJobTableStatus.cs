using FluentMigrator;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

[Migration(6)]
public class M0006CreateIndexJobTableStatus : Migration
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0006CreateIndexJobTableStatus(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Index("IX_Jobs_JobStatus")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .OnColumn("JobStatus");
    }

    public override void Down()
    {
        Delete.Index("IX_Jobs_JobStatus")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}