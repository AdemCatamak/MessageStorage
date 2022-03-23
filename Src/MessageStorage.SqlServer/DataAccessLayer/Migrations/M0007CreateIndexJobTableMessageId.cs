using FluentMigrator;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

[Migration(7)]
public class M0007CreateIndexJobTableMessageId : Migration
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0007CreateIndexJobTableMessageId(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Index("IX_Jobs_MessageId")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .OnColumn("MessageId");
    }

    public override void Down()
    {
        Delete.Index("IX_Jobs_MessageId")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}