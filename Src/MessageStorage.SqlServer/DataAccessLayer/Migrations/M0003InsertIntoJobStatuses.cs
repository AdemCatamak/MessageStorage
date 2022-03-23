using FluentMigrator;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

[Migration(version: 3)]
public class M0003InsertIntoJobStatuses : Migration
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0003InsertIntoJobStatuses(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Insert.IntoTable("JobStatuses")
              .InSchema(_repositoryContextConfiguration.Schema)
              .Row(new { Id = 1, StatusName = "Queued" })
              .Row(new { Id = 2, StatusName = "InProgress" })
              .Row(new { Id = 3, StatusName = "Completed" })
              .Row(new { Id = 4, StatusName = "Failed" });
    }

    public override void Down()
    {
        Delete.FromTable("JobStatuses")
              .InSchema(_repositoryContextConfiguration.Schema)
              .AllRows();
    }
}