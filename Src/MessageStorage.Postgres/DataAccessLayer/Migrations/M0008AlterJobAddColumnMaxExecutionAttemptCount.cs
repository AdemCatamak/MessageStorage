using FluentMigrator;

namespace MessageStorage.Postgres.DataAccessLayer.Migrations;

[Migration(8)]
public class M0008AlterJobAddColumnMaxExecutionAttemptCount : Migration
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0008AlterJobAddColumnMaxExecutionAttemptCount(PostgresRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Alter.Table("jobs")
             .InSchema(_repositoryContextConfiguration.Schema)
             .AddColumn("max_retry_count").AsInt32().NotNullable().WithDefaultValue(0);

        Rename.Column("current_execution_attempt_count")
              .OnTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .To("current_retry_count");

        Delete.Column("execute_later_than")
              .FromTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema);
    }

    public override void Down()
    {
        Alter.Table("jobs")
             .InSchema(_repositoryContextConfiguration.Schema)
             .AddColumn("execute_later_than").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Rename.Column("current_retry_count")
              .OnTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .To("current_execution_attempt_count");

        Delete.Column("max_retry_count")
              .FromTable("jobs")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}