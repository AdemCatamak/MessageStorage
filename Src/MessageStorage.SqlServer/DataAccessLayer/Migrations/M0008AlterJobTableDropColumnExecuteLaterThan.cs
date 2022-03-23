using FluentMigrator;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

[Migration(8)]
public class M0008AlterJobTableDropColumnExecuteLaterThan : Migration
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0008AlterJobTableDropColumnExecuteLaterThan(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Delete.Column("ExecuteLaterThan")
              .FromTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema);

        Rename.Column("CurrentExecutionAttemptCount")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .To("CurrentRetryCount");

        Rename.Column("MaxExecutionAttemptCount")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .To("MaxRetryCount");
    }

    public override void Down()
    {
        Rename.Column("MaxRetryCount")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .To("MaxExecutionAttemptCount");

        Rename.Column("CurrentRetryCount")
              .OnTable("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .To("CurrentExecutionAttemptCount");

        Alter.Table("Jobs")
             .AddColumn("ExecuteLaterThan").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
    }
}