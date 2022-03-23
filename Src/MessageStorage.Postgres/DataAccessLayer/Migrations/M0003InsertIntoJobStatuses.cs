using FluentMigrator;

namespace MessageStorage.Postgres.DataAccessLayer.Migrations;

[Migration(version: 3)]
public class M0003InsertIntoJobStatuses : Migration
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0003InsertIntoJobStatuses(PostgresRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Insert.IntoTable("job_statuses")
              .InSchema(_repositoryContextConfiguration.Schema)
              .Row(new {id = 1, status_name = "Queued"})
              .Row(new {id = 2, status_name = "InProgress"})
              .Row(new {id = 3, status_name = "Completed"})
              .Row(new {id = 4, status_name = "Failed"});
    }

    public override void Down()
    {
        Delete.FromTable("job_statuses")
              .InSchema(_repositoryContextConfiguration.Schema)
              .AllRows();
    }
}