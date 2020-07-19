using FluentMigrator;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    [Migration(version: 6)]
    public class _0006_CreateTable__JobTable : Migration
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0006_CreateTable__JobTable(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override void Up()
        {
            Create.Table("Jobs")
                  .InSchema(_dbRepositoryConfiguration.Schema)
                  .WithColumn("JobId").AsInt64().PrimaryKey("PK_Jobs_JobId")
                  .WithColumn("CreatedOn").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("MessageId").AsString().NotNullable()
                  .WithColumn("AssignedHandlerName").AsString(int.MaxValue).NotNullable()
                  .WithColumn("JobStatus").AsInt32().NotNullable()
                  .WithColumn("LastOperationTime").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("LastOperationInfo").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
            Delete.Table("Jobs")
                  .InSchema(_dbRepositoryConfiguration.Schema);
        }
    }
}