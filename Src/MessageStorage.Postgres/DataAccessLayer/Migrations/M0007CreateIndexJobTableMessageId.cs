using FluentMigrator;

namespace MessageStorage.Postgres.DataAccessLayer.Migrations;

[Migration(7)]
public class M0007CreateIndexJobTableMessageId : Migration
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0007CreateIndexJobTableMessageId(PostgresRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Index("IX_Jobs_MessageId")
              .OnTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .OnColumn("message_id");
    }

    public override void Down()
    {
        Delete.Index("IX_Jobs_MessageId")
              .OnTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}