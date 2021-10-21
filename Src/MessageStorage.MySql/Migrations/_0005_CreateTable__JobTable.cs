using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.MySql.Migrations
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
            Create.Table("jobs")
                  .InSchema(_repositoryConfiguration.Schema)
                  .WithColumn("id").AsGuid().PrimaryKey("PK_Jobs_Id")
                  .WithColumn("created_on").AsDateTime2().NotNullable()//.WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("message_id").AsGuid().NotNullable()
                  .WithColumn("message_handler_type_name").AsString(int.MaxValue).NotNullable()
                  .WithColumn("job_status").AsInt32().NotNullable()
                  .WithColumn("last_operation_time").AsDateTime2().NotNullable()//.WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("last_operation_info").AsString(int.MaxValue).Nullable()
                  .WithColumn("current_execution_attempt_count").AsInt32().NotNullable().WithDefaultValue(0)
                  .WithColumn("execute_later_than").AsDateTime2().NotNullable()//.WithDefault(SystemMethods.CurrentUTCDateTime)

                   ;
        }

        public override void Down()
        {
            Delete.Table("jobs")
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}