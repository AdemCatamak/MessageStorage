using FluentMigrator;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

[Migration(version: 5)]
public class M0005CreateTableJobTable : Migration
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0005CreateTableJobTable(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Table("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema)
              .WithColumn("Id").AsGuid().PrimaryKey("PK_Jobs_Id")
              .WithColumn("CreatedOn").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
              .WithColumn("MessageId").AsGuid().NotNullable()
              .WithColumn("MessageHandlerTypeName").AsString(int.MaxValue).NotNullable()
              .WithColumn("JobStatus").AsInt32().NotNullable()
              .WithColumn("LastOperationTime").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
              .WithColumn("LastOperationInfo").AsString(int.MaxValue).Nullable()
              .WithColumn("MaxExecutionAttemptCount").AsInt32().NotNullable().WithDefaultValue(0)
              .WithColumn("CurrentExecutionAttemptCount").AsInt32().NotNullable().WithDefaultValue(0)
              .WithColumn("ExecuteLaterThan").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            ;
    }

    public override void Down()
    {
        Delete.Table("Jobs")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}