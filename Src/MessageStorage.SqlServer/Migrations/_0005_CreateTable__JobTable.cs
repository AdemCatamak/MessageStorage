using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace Forgetty.SqlServer.Migrations
{
    [Migration(version: 5)]
    public class _0005_CreateTable__JobTable : Migration
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;

        public _0005_CreateTable__JobTable(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public override void Up()
        {
            Create.Table("Jobs")
                  .InSchema(_repositoryConfiguration.Schema)
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
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}